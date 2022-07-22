using DirDiff.Cli.CommandVerbs;
using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.Cli;

internal static class Shared
{
    public static async Task<DirMetaSnapshot> ReadSnapshot(string path, ISnapshotReadOptions opts)
    {
        var snapshotJsonReader = new DirMetaSnapshotJsonReader();

        var snapshotTextReader = new DirMetaSnapshotTextReader();
        snapshotTextReader.Configure(options =>
        {
            options.ReadGuess = !opts.ReadHash && !opts.ReadLastModifiedTime && !opts.ReadFileSize;

            options.ReadHash = opts.ReadHash;
            options.ReadLastModifiedTime = opts.ReadLastModifiedTime;
            options.ReadFileSize = opts.ReadFileSize;

            options.Separator = "  ";
            options.NoneValue = "-";
        });

        using var stream = File.OpenRead(path);
        try
        {
            return await snapshotJsonReader.ReadAsync(stream);
        }
        catch
        {
            stream.Position = 0;
            return await snapshotTextReader.ReadAsync(stream);
        }
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
