namespace duplexify.Application
{
    internal static class DateTimeExtensions
    {
        public static string GetSortableFileSystemName(this DateTime dateTime) => dateTime.ToString("s").Replace(":", "");
    }
}
