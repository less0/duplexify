namespace duplexify.Application
{
    internal class ConfigValidator(IConfigDirectoryService configDirectoryService) : IConfigValidator
    {
        IConfigDirectoryService _configDirectoryService = configDirectoryService;

        public void ThrowOnInvalidConfiguration()
        {
            var watchDirectory = _configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.WatchDirectory,
                Constants.DefaultWatchDirectoryName);
            var outDirectory = _configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.OutDirectory,
                Constants.DefaultOutDirectoryName);

            if(watchDirectory == outDirectory)
            {
                throw new InvalidDirectoryConfigurationException();
            }
        }
    }
}
