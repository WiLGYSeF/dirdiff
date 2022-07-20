using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Enums;

namespace DirDiff.Cli.CommandVerbs;

internal static class SnapshotVerb
{
    public static async Task Run(SnapshotOptions opts)
    {
        var snapshotBuilder = new DirMetaSnapshotBuilder();
        snapshotBuilder.Configure(options =>
        {
            options.DirectorySeparator = Path.DirectorySeparatorChar;
            options.UseFileSize = opts.UseFileSize;
            options.UseCreatedTime = true;
            options.UseLastModifiedTime = opts.UseLastModifiedTime;
            options.HashAlgorithm = opts.UseHash ? HashAlgorithm.SHA256 : null;
            options.KeepDirectoryOrder = true;
            options.ThrowIfNotFound = true;
        });

        foreach (var arg in opts.Arguments)
        {
            snapshotBuilder.AddPath(arg);
        }

        if (snapshotBuilder.SnapshotPaths.Count == 0)
        {
            foreach (var input in Shared.InputFromStream(Console.OpenStandardInput(), opts.NullSeparatedFilenameInput ? 0 : '\n'))
            {
                snapshotBuilder.AddPath(input);
            }
        }

        try
        {
            IDirMetaSnapshotWriter? snapshotWriter = opts.SnapshotFormat?.ToLower() switch
            {
                "text" => new DirMetaSnapshotTextWriter().Configure(options =>
                {
                    options.Separator = "  ";
                    options.NoneValue = "-";
                }),
                "json" => new DirMetaSnapshotJsonWriter().Configure(options =>
                {
                    options.UseUnixTimestamp = false;
                    options.WriteIndented = true;
                }),
                _ => null,
            };

            if (snapshotWriter == null)
            {
                throw new CommandVerbException(1, "unknown snapshot format");
            }

            snapshotWriter.Configure(options =>
            {
                options.WritePrefix = !opts.RemovePrefix;
                options.WriteHash = opts.UseHash;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = opts.UseLastModifiedTime;
                options.WriteFileSize = opts.UseFileSize;
            });

            DirMetaSnapshot snapshot;

            if (opts.UpdateSnapshot != null)
            {
                var origSnapshot = await Shared.ReadSnapshot(opts.UpdateSnapshot, opts);
                snapshot = await snapshotBuilder.UpdateSnapshotAsync(origSnapshot);
            }
            else
            {
                snapshot = await snapshotBuilder.CreateSnapshotAsync();
            }

            await snapshotWriter.WriteAsync(Console.OpenStandardOutput(), snapshot);
        }
        catch (DirectoryNotFoundException exception)
        {
            throw new CommandVerbException(1, $"could not find path: {exception.Message}");
        }
    }
}
