namespace duplexify.Application.Contracts.Configuration
{
    internal interface IPdfMergerConfiguration
    {
        public TimeSpan MergeRetryTimeout { get; }
        public int MergeRetryCount { get; }
        public TimeSpan StaleFileTimeout { get; }
        public string OutDirectory { get; }
        public string ErrorDirectory { get; }
    }
}
