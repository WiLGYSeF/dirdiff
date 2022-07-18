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

    public DirMetaSnapshotBuilder()
    {
        _walker = new DirWalker();
    }

    internal DirMetaSnapshotBuilder(
        IDirWalker dirWalker)
    {
        _walker = dirWalker;
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

    private void AddToSnapshot(DirMetaSnapshot snapshot, string path)
    {
        foreach (var file in _walker.Walk(path))
        {
            var entry = new DirMetaSnapshotEntry(file.Path, file.Type);

            if (Options.UseFileSize
                || Options.UseCreatedTime
                || Options.UseLastModifiedTime)
            {
                var info = new FileInfo(file.Path);

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

            if (Options.HashAlgorithm.HasValue)
            {
                entry.HashAlgorithm = Options.HashAlgorithm.Value;
                entry.Hash = Hasher.HashStream(Options.HashAlgorithm.Value, File.OpenRead(file.Path));
            }

            snapshot.AddEntry(entry);
        }
    }
}
