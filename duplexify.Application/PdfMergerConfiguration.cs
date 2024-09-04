
namespace duplexify.Application
{
    internal class PdfMergerConfiguration : IPdfMergerConfiguration
    {
        private ILogger<PdfMergerConfiguration> _logger;
        private IConfiguration _configuration;

        public PdfMergerConfiguration(ILogger<PdfMergerConfiguration> logger,
            IConfigDirectoryService configDirectoryService,
            IConfiguration configuration)
        {
            _logger = logger; 
            _configuration = configuration;

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

        private T GetValue<T>(string key, T defaultValue)
            where T : struct
        {
            var value = _configuration.GetValue(key, defaultValue);

            if(!value.Equals(defaultValue))
            {
                _logger.LogInformation($"{key} is {value}.");
            }

            return value;
        }


        public TimeSpan MergeRetryTimeout { get; init; }

        public int MergeRetryCount { get; init; }

        public TimeSpan StaleFileTimeout { get; init; }

        public string OutDirectory { get; init; }

        public string ErrorDirectory { get; init; }
    }
}
