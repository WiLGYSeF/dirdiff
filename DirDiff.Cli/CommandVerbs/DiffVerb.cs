using DirDiff.Cli.Logging;
using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshots;
using Microsoft.Extensions.Logging;

namespace DirDiff.Cli.CommandVerbs;

internal static class DiffVerb
{
    public static async Task Run(DiffOptions opts)
    {
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new ConsoleLoggerProvider());

        if (opts.Arguments.Count != 2)
        {
            throw new CommandVerbException(1, "diff requires exactly two snapshots to compare");
        }

        var args = opts.Arguments.ToList();

        var firstPath = args[0];
        var secondPath = args[1];

        IDirMetaSnapshotDiffWriter diffWriter = opts.DiffFormat?.ToLower() switch
        {
            "bash" => new DirMetaSnapshotDiffBashWriter(),
            "powershell" => new DirMetaSnapshotDiffPowershellWriter(),
            "json" => new DirMetaSnapshotDiffJsonWriter().Configure(options =>
            {
                options.WriteIndented = true;
            }),
            _ => throw new CommandVerbException(1, "unknown diff format"),
        };

        diffWriter.Configure(options =>
        {
            options.FirstPrefix = opts.FirstPrefix;
            options.SecondPrefix = opts.SecondPrefix;
        });

        DirMetaSnapshot firstSnapshot;
        DirMetaSnapshot secondSnapshot;

        try
        {
            firstSnapshot = await Shared.ReadSnapshot(firstPath);
        }
        catch (Exception ex)
        {
            throw new CommandVerbException(1, $"could not read snapshot file: {firstPath}", ex.Message);
        }

        try
        {
            secondSnapshot = await Shared.ReadSnapshot(secondPath);
        }
        catch (Exception ex)
        {
            throw new CommandVerbException(1, $"could not read snapshot file: {secondPath}", ex.Message);
        }

        var diff = secondSnapshot.Compare(
            firstSnapshot,
            !opts.NoSizeAndTimeMatch,
            !opts.UnknownNotModified,
            opts.TimeWindow.HasValue ? TimeSpan.FromSeconds(opts.TimeWindow.Value) : null);

        FileStream? fileStream = null;
        Stream outputStream;

        if (opts.OutputFilename != null)
        {
            fileStream = File.OpenWrite(opts.OutputFilename);
            outputStream = fileStream;
        }
        else
        {
            outputStream = Console.OpenStandardOutput();
        }

        await diffWriter.WriteAsync(outputStream, diff);

        fileStream?.Close();
    }
}
