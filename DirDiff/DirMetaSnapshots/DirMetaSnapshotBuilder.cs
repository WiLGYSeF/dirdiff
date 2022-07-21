using DirDiff.DirWalkers;
using DirDiff.FileInfoReaders;
using DirDiff.FileReaders;
using DirDiff.Hashers;

namespace DirDiff.DirMetaSnapshots;

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
    public DirMetaSnapshotBuilder ClearPaths()
    {
        _snapshotPaths.Clear();
        return this;
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
    /// Entries that no longer exist are removed from the snapshot.
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

        return newSnapshot;
    }

    private async Task AddToSnapshotAsync(DirMetaSnapshot snapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            snapshot.AddEntry(await CreateEntryFromResultAsync(file, skipHash: false));
        }
    }

    private async Task UpdateSnapshotAsync(DirMetaSnapshot snapshot, DirMetaSnapshot newSnapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            if (!snapshot.TryGetEntry(file.Path, out var entry))
            {
                newSnapshot.AddEntry(await CreateEntryFromResultAsync(file));
                continue;
            }

            var newEntry = await CreateEntryFromResultAsync(file, skipHash: true);

            if (newEntry.IsDifferentFrom(entry))
            {
                if (Options.HashAlgorithm.HasValue)
                {
                    SetEntryHash(newEntry);
                }
            }
            else
            {
                newEntry.CopyKnownPropertiesFrom(entry);
                if (newEntry.Hash == null && Options.HashAlgorithm.HasValue)
                {
                    SetEntryHash(newEntry);
                }
            }

            newSnapshot.AddEntry(newEntry);
        }
    }

    private async Task<DirMetaSnapshotEntry> CreateEntryFromResultAsync(DirWalkerResult result, bool skipHash = false)
    {
        var entry = new DirMetaSnapshotEntry(result.Path, result.Type);

        if (Options.UseFileSize
            || Options.UseCreatedTime
            || Options.UseLastModifiedTime)
        {
            var info = await _fileInfoReader.GetInfoAsync(result.Path);

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
            SetEntryHash(entry);
        }

        return entry;
    }

    private void SetEntryHash(DirMetaSnapshotEntry entry)
    {
        entry.HashAlgorithm = Options.HashAlgorithm!.Value;

        using var stream = _fileReader.Open(entry.Path);
        entry.Hash = _hasher.HashStream(Options.HashAlgorithm.Value, stream);
    }
}
