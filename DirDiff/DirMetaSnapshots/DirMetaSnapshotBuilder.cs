using DirDiff.DirWalkers;
using DirDiff.Hashers;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotBuilder
{
    public DirMetaSnapshotBuilderOptions Options { get; } = new();

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

    public DirMetaSnapshotBuilder Configure(Action<DirMetaSnapshotBuilderOptions> action)
    {
        action(Options);
        return this;
    }

    public DirMetaSnapshot CreateSnapshot(string path)
    {
        var snapshot = new DirMetaSnapshot(path);

        _walker.Configure(options =>
        {
            options.MinDepthLimit = Options.MinDepthLimit;
            options.MaxDepthLimit = Options.MaxDepthLimit;
            options.KeepDirectoryOrder = Options.KeepDirectoryOrder;
            options.ThrowIfNotFound = Options.ThrowIfNotFound;
        });

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

        return snapshot;
    }
}
