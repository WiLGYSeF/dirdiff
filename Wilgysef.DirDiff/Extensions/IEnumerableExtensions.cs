using System.Text;

namespace Wilgysef.DirDiff.Extensions;

internal static class IEnumerableExtensions
{
    public static string Join(this IEnumerable<string> strings, char separator)
    {
        var builder = new StringBuilder();

        foreach (var str in strings)
        {
            builder.Append(str);
            builder.Append(separator);
        }

        if (builder.Length > 0)
        {
            builder.Remove(builder.Length - 1, 1);
        }
        return builder.ToString();
    }

    public static string Join(this IEnumerable<string> strings, string separator)
    {
        var builder = new StringBuilder();

        foreach (var str in strings)
        {
            builder.Append(str);
            builder.Append(separator);
        }

        if (builder.Length > 0)
        {
            builder.Remove(builder.Length - separator.Length, separator.Length);
        }
        return builder.ToString();
    }
}
