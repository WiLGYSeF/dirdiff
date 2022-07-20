using DirDiff.DirWalkers;
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
    private readonly IHasher _hasher;

    public DirMetaSnapshotBuilder()
    {
        _walker = new DirWalker();
        _hasher = new Hasher();
    }

    internal DirMetaSnapshotBuilder(
        IDirWalker dirWalker,
        IHasher hasher)
    {
        _walker = dirWalker;
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
    /// Traverses paths and creates snapshot.
    /// </summary>
    /// <returns>Snapshot.</returns>
    public DirMetaSnapshot CreateSnapshot()
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
            AddToSnapshot(snapshot, path);
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
    public DirMetaSnapshot UpdateSnapshot(DirMetaSnapshot snapshot)
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
            UpdateSnapshot(snapshot, newSnapshot, path);
        }

        return newSnapshot;
    }

    private void AddToSnapshot(DirMetaSnapshot snapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            snapshot.AddEntry(CreateEntryFromResult(file, skipHash: false));
        }
    }

    private void UpdateSnapshot(DirMetaSnapshot snapshot, DirMetaSnapshot newSnapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            if (!snapshot.TryGetEntry(file.Path, out var entry))
            {
                newSnapshot.AddEntry(CreateEntryFromResult(file));
                continue;
            }

            var newEntry = CreateEntryFromResult(file, skipHash: true);

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

    private DirMetaSnapshotEntry CreateEntryFromResult(DirWalkerResult result, bool skipHash = false)
    {
        var entry = new DirMetaSnapshotEntry(result.Path, result.Type);

        if (Options.UseFileSize
            || Options.UseCreatedTime
            || Options.UseLastModifiedTime)
        {
            var info = new FileInfo(result.Path);

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
        entry.Hash = _hasher.HashStream(Options.HashAlgorithm.Value, File.OpenRead(entry.Path));
    }
}
