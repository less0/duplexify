namespace duplexify.Application.Contracts.Configuration
{
    internal interface IWatchDirectoryWorkerConfiguration
    {
        public TimeSpan ProcessingDelay { get; }
        public string WatchDirectory { get; }
    }
}
