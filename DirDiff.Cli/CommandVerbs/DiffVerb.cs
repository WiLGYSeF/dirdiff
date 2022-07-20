using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;

namespace DirDiff.Cli.CommandVerbs;

internal static class DiffVerb
{
    public static async Task Run(DiffOptions opts)
    {
        if (opts.Arguments.Count() != 2)
        {
            throw new CommandVerbException(1, "diff requires exactly two snapshots to compare");
        }

        var args = opts.Arguments.ToList();

        var firstPath = args[0];
        var secondPath = args[1];

        DirMetaSnapshot firstSnapshot;
        DirMetaSnapshot secondSnapshot;

        try
        {
            firstSnapshot = await ReadSnapshot(firstPath, opts);
        }
        catch
        {
            throw new CommandVerbException(1, $"could not read snapshot file: {firstPath}");
        }

        try
        {
            secondSnapshot = await ReadSnapshot(secondPath, opts);
        }
        catch
        {
            throw new CommandVerbException(1, $"could not read snapshot file: {secondPath}");
        }

        var diff = secondSnapshot.Compare(
            firstSnapshot,
            !opts.NoSizeAndTimeMatch,
            !opts.UnknownNotModified,
            opts.ModifyWindow.HasValue ? TimeSpan.FromSeconds(opts.ModifyWindow.Value) : null);

        IDirMetaSnapshotDiffWriter? diffWriter = opts.DiffFormat?.ToLower() switch
        {
            "bash" => new DirMetaSnapshotDiffBashWriter(),
            "powershell" => new DirMetaSnapshotDiffPowershellWriter(),
            "json" => new DirMetaSnapshotDiffJsonWriter().Configure(options =>
            {
                options.UseUnixTimestamp = false;
                options.WriteIndented = true;
            }),
            _ => null,
        };

        if (diffWriter == null)
        {
            throw new CommandVerbException(1, "unknown diff format");
        }

        diffWriter.Configure(options =>
        {
            options.FirstPrefix = opts.FirstPrefix;
            options.SecondPrefix = opts.SecondPrefix;
        });

        await diffWriter.WriteAsync(Console.OpenStandardOutput(), diff);
    }

    static async Task<DirMetaSnapshot> ReadSnapshot(string path, DiffOptions opts)
    {
        var snapshotJsonReader = new DirMetaSnapshotJsonReader();

        var snapshotTextReader = new DirMetaSnapshotTextReader();
        snapshotTextReader.Configure(options =>
        {
            options.ReadGuess = true;

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
}
