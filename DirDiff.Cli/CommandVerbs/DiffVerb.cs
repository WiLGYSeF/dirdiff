using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirDiff.Cli.CommandVerbs;

internal static class DiffVerb
{
    public static async Task<int> Run(DiffOptions opts)
    {
        if (opts.Arguments.Count() != 2)
        {
            Shared.WriteError($"diff requires exactly two snapshots to compare.");
            return 1;
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
            Shared.WriteError($"could not read snapshot file: {firstPath}");
            return 1;
        }

        try
        {
            secondSnapshot = await ReadSnapshot(secondPath, opts);
        }
        catch
        {
            Shared.WriteError($"could not read snapshot file: {secondPath}");
            return 1;
        }

        var diff = secondSnapshot.Compare(
            firstSnapshot,
            !opts.NoSizeAndTimeMatch,
            !opts.UnknownNotModified,
            opts.ModifyWindow.HasValue ? TimeSpan.FromSeconds(opts.ModifyWindow.Value) : null);

        IDirMetaSnapshotDiffWriter? diffWriter = opts.DiffFormat?.ToLower() switch
        {
            "bash" => new DirMetaSnapshotDiffBashWriter(),
            "json" => new DirMetaSnapshotDiffJsonWriter().Configure(options =>
            {
                options.UseUnixTimestamp = false;
                options.WriteIndented = true;
            }),
            _ => null,
        };

        if (diffWriter == null)
        {
            Shared.WriteError("unknown diff format");
            return 1;
        }

        diffWriter.Configure(options =>
        {
            options.FirstPrefix = opts.FirstPrefix;
            options.SecondPrefix = opts.SecondPrefix;
        });

        await diffWriter.WriteAsync(Console.OpenStandardOutput(), diff);
        return 0;
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
