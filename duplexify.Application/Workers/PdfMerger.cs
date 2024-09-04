using duplexify.Application.Contracts;
using duplexify.Application.Contracts.Configuration;
using Polly;
using Polly.Retry;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace duplexify.Application.Workers
{
    internal class PdfMerger : BackgroundService, IPdfMerger
    {
        private readonly ILogger<PdfMerger> _logger;
        private IPdfMergerConfiguration _configuration;
        private ConcurrentQueue<string> _processingQueue = new();
        private string _currentErrorDirectory = null!;
        private RetryPolicy<bool> _mergeRetryPolicy;

        public PdfMerger(ILogger<PdfMerger> logger, 
            IPdfMergerConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            SetUpMergeRetryPolicy();

            _logger.LogInformation("Writing to directory {0}", _configuration.OutDirectory);
            _logger.LogInformation("Writing corrupt PDFs to {0}", _configuration.ErrorDirectory);
        }

        private bool SingleFileInQueueIsStale => _processingQueue.Count == 1
                            && _processingQueue.TryPeek(out var filePath)
                            && DateTime.UtcNow - File.GetCreationTimeUtc(filePath) > _configuration.StaleFileTimeout;

        [MemberNotNull(nameof(_mergeRetryPolicy))]
        private void SetUpMergeRetryPolicy()
        {
            _mergeRetryPolicy = Policy.HandleResult(false)
                .WaitAndRetry(_configuration.MergeRetryCount, _ => _configuration.MergeRetryTimeout, (_, _, currentRetryCount, _) => _logger.LogError($"Failed merging files, retrying. ({currentRetryCount})"));
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
            string outFile = Path.Combine(_configuration.OutDirectory, $"{DateTime.Now.GetSortableFileSystemName()}.pdf");

            _logger.LogInformation($"Merging {fileA} and {fileB}");

            var mergedSuccessfully = _mergeRetryPolicy.Execute(() => MergeToFile(fileA, fileB, outFile));

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
            var uniqueErrorDirectory = Path.Combine(_configuration.ErrorDirectory, DateTime.Now.GetSortableFileSystemName());

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
