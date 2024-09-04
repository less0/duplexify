using duplexify.Application.Contracts.Configuration;

namespace duplexify.Application.Configuration
{
    internal class PdfMergerConfiguration : ConfigurationBase, IPdfMergerConfiguration
    {
        private ILogger<PdfMergerConfiguration> _logger;

        public PdfMergerConfiguration(ILogger<PdfMergerConfiguration> logger,
            IConfigDirectoryService configDirectoryService,
            IConfiguration configuration)
            : base(logger, configuration) 
        {
            _logger = logger;

            OutDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.OutDirectory,
                Constants.DefaultOutDirectoryName);
            ErrorDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.ErrorDirectory,
                Constants.DefaultErrorDirectoryName);

            StaleFileTimeout = GetValue(nameof(StaleFileTimeout), TimeSpan.FromHours(1));
            MergeRetryTimeout = GetValue(nameof(MergeRetryTimeout), TimeSpan.FromSeconds(5));
            MergeRetryCount = GetValue(nameof(MergeRetryCount), 5);
        }

        public TimeSpan MergeRetryTimeout { get; init; }

        public int MergeRetryCount { get; init; }

        public TimeSpan StaleFileTimeout { get; init; }

        public string OutDirectory { get; init; }

        public string ErrorDirectory { get; init; }
    }
}
