using System.ComponentModel;
using System.Diagnostics;

namespace duplexify.Application
{
    internal class PdfMerger : IPdfMerger
    {
        private readonly ILogger<PdfMerger> _logger;
        private string _outDirectory;
        private string _errorDirectory;
        private Queue<string> _processingQueue = new();

        public PdfMerger(ILogger<PdfMerger> logger, IConfigDirectoryService configDirectoryService)
        {
            _logger = logger;

            _outDirectory = configDirectoryService.GetDirectory("OutDirectory", "out");
            _errorDirectory = configDirectoryService.GetDirectory("ErrorDirectory", "error");

            _logger.LogInformation("Writing to directory {0}", _outDirectory);
            _logger.LogInformation("Writing corrupt PDFs to {0}", _errorDirectory);
        }

        public void EnqueueForMerging(string path)
        {
            _logger.LogInformation("Processing {0}", path);

            _processingQueue.Enqueue(path);

            if(_processingQueue.Count >= 2)
            {
                MergeFirstTwoFilesFromQueue();
            }
        }

        private void MergeFirstTwoFilesFromQueue()
        {
            string fileA = _processingQueue.Dequeue();
            string fileB = _processingQueue.Dequeue();
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
                var process = System.Diagnostics.Process.Start(new ProcessStartInfo("pdftk")
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
