using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.Cli;

internal static class Shared
{
    public static async Task<DirMetaSnapshot> ReadSnapshot(string path)
    {
        var snapshotJsonReader = new DirMetaSnapshotJsonReader();

        var snapshotYamlReader = new DirMetaSnapshotYamlReader();

        var snapshotTextReader = new DirMetaSnapshotTextReader();
        snapshotTextReader.Configure(options =>
        {
            options.ReadGuess = true;

            options.Separator = "  ";
            options.NoneValue = "-";
        });

        DirMetaSnapshot? snapshot = null;
        Exception? lastException = null;

        using var stream = File.OpenRead(path);
        foreach (var reader in new IDirMetaSnapshotReader[] { snapshotJsonReader, snapshotYamlReader, snapshotTextReader })
        {
            try
            {
                snapshot = await reader.ReadAsync(stream);
                break;
            }
            catch (Exception exception)
            {
                stream.Position = 0;
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
