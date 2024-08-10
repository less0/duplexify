namespace duplexify.Application
{
    /// <summary>
    /// Contract for a class that merges PDF files. The files are processed one by one and as soon
    /// as there are at least two files, the first two files (FIFO) are merged and the source files 
    /// are deleted.
    /// </summary>
    public interface IPdfMerger
    {
        /// <summary>
        /// Enqueues a PDF file for merging. As soon as there are at least two files, the first two
        /// files are merged. This method is not nescessarily thread safe.
        /// </summary>
        /// <param name="path">
        /// The path of the file to enqueue for merging.
        /// </param>
        public void EnqueueForMerging(string path);
    }
}
