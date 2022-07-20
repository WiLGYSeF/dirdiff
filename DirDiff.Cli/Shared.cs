using System.Text;

namespace DirDiff.Cli;

internal static class Shared
{
    public static IEnumerable<string> InputFromStream(Stream stream, int delimiter)
    {
        var input = new StringBuilder();
        int @byte;

        while ((@byte = stream.ReadByte()) != -1)
        {
            if (@byte == delimiter)
            {
                if (input.Length > 0)
                {
                    yield return input.ToString();
                    input.Clear();
                }
            }
            else
            {
                input.Append((char)@byte);
            }
        }

        if (input.Length > 0)
        {
            yield return input.ToString();
        }
    }

    public static void WriteError(string message)
    {
        Console.Error.WriteLine("error: " + message);
    }
}
