namespace duplexify.Application
{
    internal static class Constants
    {
        public const string DefaultWatchDirectoryName = "in";
        public const string DefaultOutDirectoryName = "out";
        public const string DefaultErrorDirectoryName = "error";

        public static class ConfigurationKeys
        {
            public const string WatchDirectory = nameof(WatchDirectory);
            public const string OutDirectory = nameof(OutDirectory);
            public const string ErrorDirectory = nameof(ErrorDirectory);
        }
    }
}
