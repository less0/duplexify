using Polly;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace duplexify.Application.Workers
{
    internal class PdfMerger : BackgroundService, IPdfMerger
    {
        private readonly ILogger<PdfMerger> _logger;
        private string _outDirectory;
        private string _errorDirectory;
        private TimeSpan _staleFileTimeout;
        private ConcurrentQueue<string> _processingQueue = new();
        private string _currentErrorDirectory = null!;

        public PdfMerger(ILogger<PdfMerger> logger, 
            IConfigDirectoryService configDirectoryService,
            IConfiguration configuration)
        {
            _logger = logger;

            _outDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.OutDirectory,
                Constants.DefaultOutDirectoryName);
            _errorDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.ErrorDirectory,
                Constants.DefaultErrorDirectoryName);

            _staleFileTimeout = configuration.GetValue("StaleFileTimeout", TimeSpan.FromHours(1));

            _logger.LogInformation("Writing to directory {0}", _outDirectory);
            _logger.LogInformation("Writing corrupt PDFs to {0}", _errorDirectory);
        }

        public void EnqueueForMerging(string path)
        {
            _processingQueue.Enqueue(path);
            _logger.LogInformation("Enqueued {0}", path);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                RemoveStaleFiles();
                MergeFirstTwoFilesFromQueue();
                await Task.Delay(1000);
            }
        }

        private void RemoveStaleFiles()
        {
            if (SingleFileInQueueIsStale)
            {
                if(!_processingQueue.TryDequeue(out var staleFilePath))
                {
                    throw new InvalidOperationException();
                }

                File.Delete(staleFilePath);
                _logger.LogInformation($"Deleted stale file {staleFilePath}.");
            }
        }

        private bool SingleFileInQueueIsStale => _processingQueue.Count == 1
                            && _processingQueue.TryPeek(out var filePath)
                            && DateTime.Now - File.GetLastWriteTime(filePath) > _staleFileTimeout;

        private void MergeFirstTwoFilesFromQueue()
        {
            if (_processingQueue.Count < 2)
            {
                return;
            }

            // These calls should not ever return false. 
            if (!_processingQueue.TryDequeue(out var fileA)
            || !_processingQueue.TryDequeue(out var fileB))
            {
                throw new InvalidOperationException();
            }

            MergeFiles(fileA, fileB);
        }

        private void MergeFiles(string fileA, string fileB)
        {
            string outFile = Path.Combine(_outDirectory, $"{DateTime.Now.GetSortableFileSystemName()}.pdf");

            _logger.LogInformation($"Merging {fileA} and {fileB}");

            var mergedSuccessfully = Policy.HandleResult(false)
                .WaitAndRetry(5, _ => TimeSpan.FromSeconds(5), (_, _) => _logger.LogError("Failed merging files, retrying."))
                .Execute(() => MergeToFile(fileA, fileB, outFile));

            if (mergedSuccessfully)
            {
                DeleteSourceFiles(fileA, fileB);
                _logger.LogInformation($"Merged files to {outFile}");
            }
            else
            {
                CreateUniqueErrorDirectory();

                // We can't continue with the files still being around
                Policy.Handle<IOException>().RetryForever().Execute(() => MoveToErrorDirectory(fileA));
                Policy.Handle<IOException>().RetryForever().Execute(() => MoveToErrorDirectory(fileB));

                _logger.LogError("Error occurred, moved files to error directory {0}", _currentErrorDirectory);
            }
        }

        private void CreateUniqueErrorDirectory()
        {
            var uniqueErrorDirectory = Path.Combine(_errorDirectory, DateTime.Now.GetSortableFileSystemName());

            if (!Directory.Exists(uniqueErrorDirectory))
            {
                Directory.CreateDirectory(uniqueErrorDirectory);
            }

            _currentErrorDirectory = uniqueErrorDirectory;
        }

        private void MoveToErrorDirectory(string filePath)
        {
            var targetPath = Path.Combine(_currentErrorDirectory, Path.GetFileName(filePath));
            File.Move(filePath, targetPath);
        }

        private static void DeleteSourceFiles(string fileA, string fileB)
        {
            File.Delete(fileA);
            File.Delete(fileB);
        }

        private bool MergeToFile(string fileA, string fileB, string outFile)
        {
            try
            {
                var pdftkProcess = Process.Start(new ProcessStartInfo("pdftk")
                {
                    Arguments = $"A=\"{fileA}\" B=\"{fileB}\" shuffle A Bend-1 output \"{outFile}\""
                });

                pdftkProcess?.WaitForExit();
                _logger.LogInformation($"Exit code {pdftkProcess?.ExitCode}");
                return pdftkProcess?.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }
    }
}
