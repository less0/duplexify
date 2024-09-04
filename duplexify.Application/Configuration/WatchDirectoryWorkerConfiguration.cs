using duplexify.Application.Contracts.Configuration;

namespace duplexify.Application.Configuration
{
    internal class WatchDirectoryWorkerConfiguration : ConfigurationBase, IWatchDirectoryWorkerConfiguration
    {
        public WatchDirectoryWorkerConfiguration(ILogger<WatchDirectoryWorkerConfiguration> logger, IConfiguration configuration, IConfigDirectoryService configDirectoryService) : base(logger, configuration)
        {
            WatchDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.WatchDirectory,
                Constants.DefaultWatchDirectoryName);

            ProcessingDelay = GetValue(nameof(ProcessingDelay), TimeSpan.Zero);
        }

        public TimeSpan ProcessingDelay { get; init; }

        public string WatchDirectory { get; init; }
    }
}
