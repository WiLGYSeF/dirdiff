namespace DirDiff.Extensions;

internal static class DateTimeExtensions
{
    /// <summary>
    /// Checks if the difference between two <see cref="DateTime"/>s are within a time window.
    /// </summary>
    /// <param name="dateTime">DateTime.</param>
    /// <param name="other">Other DateTime.</param>
    /// <param name="window">Maximum difference in times.</param>
    /// <returns><see langword="true"/> if the difference is within the time window, otherwise <see langword="false"/>.</returns>
    public static bool Within(this DateTime dateTime, DateTime other, TimeSpan window)
    {
        return (dateTime - other).Duration() <= window;
    }
}
