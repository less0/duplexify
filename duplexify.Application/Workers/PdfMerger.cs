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
        private ConcurrentQueue<string> _processingQueue = new();

        public PdfMerger(ILogger<PdfMerger> logger, IConfigDirectoryService configDirectoryService)
        {
            _logger = logger;

            _outDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.OutDirectory,
                Constants.DefaultOutDirectoryName);
            _errorDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.ErrorDirectory,
                Constants.DefaultErrorDirectoryName);

            _logger.LogInformation("Writing to directory {0}", _outDirectory);
            _logger.LogInformation("Writing corrupt PDFs to {0}", _errorDirectory);
        }

        public void EnqueueForMerging(string path)
        {
            _processingQueue.Enqueue(path);
            _logger.LogInformation("Enqueued {0}", path);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    MergeFirstTwoFilesFromQueue();
                }
            }, stoppingToken);
        }

        private void MergeFirstTwoFilesFromQueue()
        {
            if (_processingQueue.Count < 2)
            {
                return;
            }

            // These calls should not ever return false. 
            if(!_processingQueue.TryDequeue(out var fileA)
            || !_processingQueue.TryDequeue(out var fileB))
            {
                throw new InvalidOperationException();
            }

            string outFile = Path.Combine(_outDirectory, $"{DateTime.Now.GetSortableFileSystemName()}.pdf");

            _logger.LogInformation($"Merging {fileA} and {fileB}");
            if (MergeFiles(fileA, fileB, outFile))
            {
                DeleteSourceFiles(fileA, fileB);
                _logger.LogInformation($"Merged files to {outFile}");
            }
            else
            {
                MoveToErrorDirectory(fileA, fileB, out var directory);
                _logger.LogError("Error occurred, moved files to error directory {0}", directory);
            }
        }

        private void MoveToErrorDirectory(string fileA, string fileB, out string directory)
        {
            var uniqueErrorDirectory = Path.Combine(_errorDirectory, DateTime.Now.GetSortableFileSystemName());

            if (!Directory.Exists(uniqueErrorDirectory))
            {
                Directory.CreateDirectory(uniqueErrorDirectory);
            }

            var errorFileA = Path.Combine(uniqueErrorDirectory, Path.GetFileName(fileA));
            var errorFileB = Path.Combine(uniqueErrorDirectory, Path.GetFileName(fileB));

            File.Move(fileA, errorFileA);
            File.Move(fileB, errorFileB);

            directory = uniqueErrorDirectory;
        }

        private static void DeleteSourceFiles(string fileA, string fileB)
        {
            File.Delete(fileA);
            File.Delete(fileB);
        }

        private bool MergeFiles(string fileA, string fileB, string outFile)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo("pdftk")
                {
                    Arguments = $"A=\"{fileA}\" B=\"{fileB}\" shuffle A Bend-1 output \"{outFile}\""
                });

                process?.WaitForExit();
                _logger.LogInformation($"Exit code {process?.ExitCode}");
                return process?.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }
    }
}
