using System.Text;

namespace DirDiff.Extensions;

public static class IEnumerableExtensions
{
    public static string JoinAsString(this IEnumerable<string> strings, string concat)
    {
        var builder = new StringBuilder();

        foreach (var str in strings)
        {
            builder.Append(str);
            builder.Append(concat);
        }

        builder.Remove(builder.Length - concat.Length, concat.Length);
        return builder.ToString();
    }
}
