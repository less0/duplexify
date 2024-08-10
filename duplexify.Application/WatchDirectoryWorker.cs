namespace duplexify.Application;

/// <summary>
/// Worker that watches a specific directory for PDF files to come in and enqueues them with a
/// <see cref="IPdfMerger"/>. Watching the directory is intentionally <em>not</em> implemented 
/// with a <see cref="FileSystemWatcher"/> to assure an increased compatability across systems 
/// and file systems.
/// </summary>
public class WatchDirectoryWorker : BackgroundService
{
    private readonly ILogger<WatchDirectoryWorker> _logger;
    private readonly IPdfMerger _pdfMerger;
    private readonly string _watchDirectory;
    private readonly HashSet<string> _filesNotToProcess = new();

    public WatchDirectoryWorker(ILogger<WatchDirectoryWorker> logger, 
        IConfigDirectoryService configDirectoryService, 
        IPdfMerger pdfMerger)
    {
        _logger = logger;
        _pdfMerger = pdfMerger;

        _watchDirectory = configDirectoryService.GetDirectory(
            Constants.ConfigurationKeys.WatchDirectory, 
            Constants.DefaultWatchDirectoryName);

        _logger.LogInformation("Watching directory {0}", _watchDirectory);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var files = Directory.GetFiles(_watchDirectory, "*.pdf", new EnumerationOptions() { RecurseSubdirectories = false });
            files = files.OrderBy(File.GetCreationTime).ToArray();

            // Remove files that do not exist from list of files not to process, to allow 
            // to process files with the same names later.
            foreach (var fileNotToProcess in _filesNotToProcess.Where(f => !files.Contains(f)))
            {
                _filesNotToProcess.Remove(fileNotToProcess);
            }

            foreach(var fileToProcess in files.Where(f => !_filesNotToProcess.Contains(f)))
            {
                // Add file to list of files that should not be processed to prevent the file
                // to be processed in the next iteration
                _filesNotToProcess.Add(fileToProcess);
                _pdfMerger.EnqueueForMerging(fileToProcess);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
