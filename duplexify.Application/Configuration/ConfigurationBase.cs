namespace duplexify.Application.Configuration
{
    internal class ConfigurationBase
    {
        public ConfigurationBase(ILogger logger, IConfiguration configuration) {
        
            Configuration = configuration;
            Logger = logger;
        }

        private IConfiguration Configuration { get; init; }

        private ILogger Logger { get; init; }

        protected T GetValue<T>(string key, T defaultValue)
            where T : struct
        {
            var value = Configuration.GetValue(key, defaultValue);

            if (!value.Equals(defaultValue))
            {
                Logger.LogInformation($"{key} is {value}.");
            }

            return value;
        }
    }
}