using Wilgysef.DirDiff.Cli.Logging;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using Wilgysef.DirDiff.DirWalkers;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.FileInfoReaders;
using Wilgysef.DirDiff.FileReaders;
using Wilgysef.DirDiff.Hashers;
using Microsoft.Extensions.Logging;

namespace Wilgysef.DirDiff.Cli.CommandVerbs;

internal static class SnapshotVerb
{
    public static async Task Run(SnapshotOptions opts)
    {
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new ConsoleLoggerProvider());

        HashAlgorithm? hashAlgorithm = HashAlgorithm.SHA256;
        if (opts.HashAlgorithm != null)
        {
            hashAlgorithm = Shared.ParseHashAlgorithm(opts.HashAlgorithm);
            if (!hashAlgorithm.HasValue)
            {
                throw new CommandVerbException(1, "unknown hash algorithm");
            }
        }

        var snapshotBuilder = new DirMetaSnapshotBuilder(
            new DirWalker(),
            new FileReader(),
            new FileInfoReader(),
            new Hasher());
        snapshotBuilder.Configure(options =>
        {
            // TODO: check this?
            options.DirectorySeparator = Path.DirectorySeparatorChar;
            options.UseFileSize = opts.UseFileSize;
            options.UseCreatedTime = true;
            options.UseLastModifiedTime = opts.UseLastModifiedTime;
            options.HashAlgorithm = opts.UseHash ? hashAlgorithm.Value : null;
            options.TimeWindow = TimeSpan.FromSeconds(opts.TimeWindow ?? 0);
            options.UpdateKeepRemoved = opts.UpdateNoRemove;
            options.UpdatePrefix = opts.UpdatePrefix;
            options.KeepDirectoryOrder = true;
            options.ThrowIfNotFound = true;
        });

        if (opts.Verbose > 0)
        {
            snapshotBuilder.Logger = loggerFactory.CreateLogger<DirMetaSnapshotBuilder>();
        }

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
            IDirMetaSnapshotWriter snapshotWriter = opts.SnapshotFormat?.ToLower() switch
            {
                "text" => new DirMetaSnapshotTextWriter().Configure(options =>
                {
                    options.Separator = "  ";
                    options.NoneValue = "-";
                }),
                "json" => new DirMetaSnapshotJsonWriter().Configure(options =>
                {
                    options.WriteIndented = true;
                }),
                "yaml" => new DirMetaSnapshotYamlWriter(),
                _ => throw new CommandVerbException(1, "unknown snapshot format"),
            };

            snapshotWriter.Configure(options =>
            {
                options.WritePrefix = !opts.RemovePrefix;
                options.DirectorySeparator = opts.DirectorySeparator;
                options.SortByPath = opts.SortByPath;
                options.WriteHash = opts.UseHash;
                options.WriteHashAlgorithm = opts.UseHash;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = opts.UseLastModifiedTime;
                options.WriteFileSize = opts.UseFileSize;
            });

            DirMetaSnapshot snapshot;

            if (opts.UpdateSnapshot != null)
            {
                var origSnapshot = await Shared.ReadSnapshot(opts.UpdateSnapshot);
                snapshot = await snapshotBuilder.UpdateSnapshotAsync(origSnapshot);
            }
            else
            {
                snapshot = await snapshotBuilder.CreateSnapshotAsync();
            }

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

            await snapshotWriter.WriteAsync(outputStream, snapshot);

            fileStream?.Close();
        }
        catch (DirectoryNotFoundException exception)
        {
            throw new CommandVerbException(1, $"could not find directory: {exception.Message}");
        }
        catch (FileNotFoundException exception)
        {
            throw new CommandVerbException(1, $"could not find file: {exception.Message}");
        }
    }
}
