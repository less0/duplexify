namespace duplexify.Application
{
    public interface IConfigDirectoryService
    {
        /// <summary>
        /// Gets a directory with the given key from the configuration. The directory is created, if
        /// it does not exist. If there is no such key, a subdirectory of the current directory with 
        /// the default directory name is created and returned. 
        /// </summary>
        /// <param name="configKey">The configuration key to get the directory path from.</param>
        /// <param name="defaultDirectoryName">
        /// The name of the default directory to create if the configuration key does not exist in 
        /// the configuration.
        /// </param>
        /// <returns>
        /// The directory from <paramref name="configKey"/> if the key exists in the configuration,
        /// the subdirectory of the current working directory with the name 
        /// <paramref name="defaultDirectoryName"/>. 
        /// </returns>
        string GetDirectory(string configKey, string defaultDirectoryName);
    }
}
