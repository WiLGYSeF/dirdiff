using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.Cli;

internal static class Shared
{
    public static async Task<DirMetaSnapshot> ReadSnapshot(string path)
    {
        var readers = new List<IDirMetaSnapshotReader>
        {
            new DirMetaSnapshotJsonReader(),
            new DirMetaSnapshotYamlReader(),
            new DirMetaSnapshotTextReader().Configure(options =>
            {
                options.ReadGuess = true;

                options.Separator = "  ";
                options.NoneValue = "-";
            })
        };

        DirMetaSnapshot? snapshot = null;
        Exception? lastException = null;

        foreach (var reader in readers)
        {
            try
            {
                using var stream = File.OpenRead(path);
                snapshot = await reader.ReadAsync(stream);
                lastException = null;
                break;
            }
            catch (Exception exception)
            {
                lastException = exception;
            }
        }

        if (lastException != null)
        {
            throw lastException;
        }

        return snapshot!;
    }

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
}
