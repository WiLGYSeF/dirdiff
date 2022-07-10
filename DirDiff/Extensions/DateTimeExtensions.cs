namespace DirDiff.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Convert to UNIX timestamp.
    /// </summary>
    /// <param name="dateTime">DateTime.</param>
    /// <returns>UNIX timestamp in seconds.</returns>
    public static double ToUnixTimestamp(this DateTime dateTime)
    {
        return (dateTime - DateTime.UnixEpoch).TotalSeconds;
    }
}
