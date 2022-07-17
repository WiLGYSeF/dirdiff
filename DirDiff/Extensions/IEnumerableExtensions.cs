using System.Text;

namespace DirDiff.Extensions;

internal static class IEnumerableExtensions
{
    public static string Join(this IEnumerable<string> strings, string separator)
    {
        var builder = new StringBuilder();

        foreach (var str in strings)
        {
            builder.Append(str);
            builder.Append(separator);
        }

        builder.Remove(builder.Length - separator.Length, separator.Length);
        return builder.ToString();
    }
}
