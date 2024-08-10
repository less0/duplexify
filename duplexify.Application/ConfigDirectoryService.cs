namespace duplexify.Application
{
    internal class ConfigDirectoryService : IConfigDirectoryService
    {
        IConfiguration _configuration;

        public ConfigDirectoryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetDirectory(string configKey, string defaultDirectoryName)
        {
            var directory = _configuration.GetValue<string>(configKey)
                ?? Path.Combine(Directory.GetCurrentDirectory(), defaultDirectoryName);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }
    }
}
