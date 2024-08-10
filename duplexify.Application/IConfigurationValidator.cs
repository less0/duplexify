namespace duplexify.Application
{
    internal interface IConfigurationValidator
    {
        /// <summary> 
        /// Throws an exception if the configuration is invalid.
        /// </summary>
        /// <exception cref="InvalidDirectoryConfigurationException">
        /// The directory configuration is invalid, i.e. the same directories are chosen for in-
        /// and output.
        /// </exception>
        void ThrowOnInvalidConfiguration();
    }
}
