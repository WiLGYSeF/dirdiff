using Wilgysef.DirDiff.DirWalkers;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.FileInfoReaders;
using Wilgysef.DirDiff.FileReaders;
using Wilgysef.DirDiff.Hashers;
using Microsoft.Extensions.Logging;

namespace Wilgysef.DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotBuilder
{
    /// <summary>
    /// Snapshot builder options.
    /// </summary>
    public DirMetaSnapshotBuilderOptions Options { get; } = new();

    /// <summary>
    /// Paths that will be traversed for the snapshot.
    /// </summary>
    public IReadOnlyList<string> SnapshotPaths => _snapshotPaths;

    public ILogger? Logger { get; set; }

    private readonly List<string> _snapshotPaths = new();

    private readonly IDirWalker _walker;
    private readonly IFileReader _fileReader;
    private readonly IFileInfoReader _fileInfoReader;
    private readonly IHasher _hasher;

    public DirMetaSnapshotBuilder(
        IDirWalker dirWalker,
        IFileReader fileReader,
        IFileInfoReader fileInfoReader,
        IHasher hasher)
    {
        _walker = dirWalker;
        _fileReader = fileReader;
        _fileInfoReader = fileInfoReader;
        _hasher = hasher;
    }

    /// <summary>
    /// Configures snapshot builder.
    /// </summary>
    /// <param name="action">Configure aciton.</param>
    /// <returns></returns>
    public DirMetaSnapshotBuilder Configure(Action<DirMetaSnapshotBuilderOptions> action)
    {
        action(Options);
        return this;
    }

    /// <summary>
    /// Adds a logger to the snapshot builder.
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <returns></returns>
    public DirMetaSnapshotBuilder WithLogger(ILogger logger)
    {
        Logger = logger;
        return this;
    }

    /// <summary>
    /// Adds path that will be traversed in snapshot.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns></returns>
    public DirMetaSnapshotBuilder AddPath(string path)
    {
        _snapshotPaths.Add(path);
        return this;
    }

    /// <summary>
    /// Removes all paths from being traversed in snapshot.
    /// </summary>
    /// <returns></returns>
    public void ClearPaths()
    {
        _snapshotPaths.Clear();
    }

    /// <summary>
    /// Traverses paths and creates snapshot.
    /// </summary>
    /// <returns>Snapshot.</returns>
    public async Task<DirMetaSnapshot> CreateSnapshotAsync()
    {
        var snapshot = new DirMetaSnapshot(Options.DirectorySeparator);

        _walker.Configure(options =>
        {
            options.MinDepthLimit = Options.MinDepthLimit;
            options.MaxDepthLimit = Options.MaxDepthLimit;
            options.KeepDirectoryOrder = Options.KeepDirectoryOrder;
            options.ThrowIfNotFound = Options.ThrowIfNotFound;
        });

        foreach (var path in _snapshotPaths)
        {
            await AddToSnapshotAsync(snapshot, path);
        }

        return snapshot;
    }

    /// <summary>
    /// Traverses paths and updates the given snapshot.
    /// <para>
    /// New entries are added to the snapshot.
    /// Entries that no longer exist are removed from the snapshot, unless <see cref="DirMetaSnapshotBuilderOptions.UpdateKeepRemoved"/> is <see langword="true"/>.
    /// Existing entries are compared with the last modified time, file size, and hash, and are updated accordingly.
    /// </para>
    /// </summary>
    /// <param name="snapshot">Snapshot to update.</param>
    /// <returns>New snapshot with updates.</returns>
    public async Task<DirMetaSnapshot> UpdateSnapshotAsync(DirMetaSnapshot snapshot)
    {
        var newSnapshot = new DirMetaSnapshot(Options.DirectorySeparator);

        _walker.Configure(options =>
        {
            options.MinDepthLimit = Options.MinDepthLimit;
            options.MaxDepthLimit = Options.MaxDepthLimit;
            options.KeepDirectoryOrder = Options.KeepDirectoryOrder;
            options.ThrowIfNotFound = Options.ThrowIfNotFound;
        });

        foreach (var path in _snapshotPaths)
        {
            await UpdateSnapshotAsync(snapshot, newSnapshot, path);
        }

        if (Options.UpdateKeepRemoved)
        {
            foreach (var entry in snapshot.Entries)
            {
                var path = snapshot.ChangePathDirectorySeparator(entry.Path, newSnapshot.DirectorySeparator);

                if (!newSnapshot.ContainsPath(path))
                {
                    var copy = entry.Copy();
                    copy.Path = path;
                    newSnapshot.AddEntry(copy);
                }
            }
        }

        return newSnapshot;
    }

    private async Task AddToSnapshotAsync(DirMetaSnapshot snapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            Logger?.LogInformation("adding: {path}", file.Path);

            snapshot.AddEntry(await CreateEntryAsync(file));
        }
    }

    private async Task UpdateSnapshotAsync(DirMetaSnapshot snapshot, DirMetaSnapshot newSnapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            var snapshotPath = newSnapshot.ChangePathDirectorySeparator(file.Path, snapshot.DirectorySeparator);
            var newPath = file.Path;

            if (Options.UpdatePrefix != null)
            {
                if (!newPath.StartsWith(Options.UpdatePrefix))
                {
                    throw new InvalidOperationException($"Entry path does not start with prefix: {newPath}");
                }

                newPath = newPath[Options.UpdatePrefix.Length..];
                snapshotPath = snapshot.Prefix + snapshot.ChangePathDirectorySeparator(newPath, snapshot.DirectorySeparator);
            }

            DirMetaSnapshotEntry newEntry;

            if (snapshot.TryGetEntry(snapshotPath, out var entry))
            {
                newEntry = await CreateEntryAsync(file, skipHash: true);

                if (newEntry.IsDifferentFrom(entry, timeWindow: Options.TimeWindow, ignorePath: true))
                {
                    Logger?.LogInformation("updating existing entry: {path}", file.Path);

                    if (Options.HashAlgorithm.HasValue)
                    {
                        Logger?.LogInformation("hashing contents: {path}", file.Path);

                        await SetEntryHashAsync(newEntry);
                    }
                }
                else
                {
                    newEntry.CopyKnownPropertiesFrom(entry);
                    if (newEntry.Hash == null && Options.HashAlgorithm.HasValue)
                    {
                        Logger?.LogInformation("updating existing entry: {path}", file.Path);
                        Logger?.LogInformation("hashing contents: {path}", file.Path);

                        await SetEntryHashAsync(newEntry);
                    }
                    else
                    {
                        Logger?.LogInformation("using existing entry: {path}", file.Path);
                    }
                }
            }
            else
            {
                Logger?.LogInformation("updating with new entry: {path}", file.Path);

                newEntry = await CreateEntryAsync(file);
            }

            if (Options.UpdatePrefix != null)
            {
                newEntry.Path = snapshot.ChangePathDirectorySeparator(snapshot.Prefix!, newSnapshot.DirectorySeparator) + newPath;
            }

            newSnapshot.AddEntry(newEntry);
        }
    }

    private async Task<DirMetaSnapshotEntry> CreateEntryAsync(DirWalkerResult result, bool skipHash = false)
    {
        return await CreateEntryAsync(result.Path, result.Type, skipHash);
    }

    private async Task<DirMetaSnapshotEntry> CreateEntryAsync(string path, FileType type, bool skipHash = false)
    {
        var entry = new DirMetaSnapshotEntry(path, type);

        if (Options.UseFileSize
            || Options.UseCreatedTime
            || Options.UseLastModifiedTime)
        {
            var info = await _fileInfoReader.GetInfoAsync(path);

            if (Options.UseFileSize)
            {
                entry.FileSize = info.Length;
            }

            if (Options.UseCreatedTime)
            {
                entry.CreatedTime = info.CreationTimeUtc;
            }
            if (Options.UseLastModifiedTime)
            {
                entry.LastModifiedTime = info.LastWriteTimeUtc;
            }
        }

        if (!skipHash && Options.HashAlgorithm.HasValue)
        {
            await SetEntryHashAsync(entry);
        }

        return entry;
    }

    private async Task SetEntryHashAsync(DirMetaSnapshotEntry entry)
    {
        entry.HashAlgorithm = Options.HashAlgorithm!.Value;

        using var stream = _fileReader.Open(entry.Path);
        entry.Hash = await _hasher.HashStreamAsync(Options.HashAlgorithm.Value, stream);
    }
}
