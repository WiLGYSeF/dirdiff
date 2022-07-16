using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotReaders;

public interface IDirMetaSnapshotReader
{
    DirMetaSnapshotReaderOptions Options { get; }

    IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action);

    Task<DirMetaSnapshot> ReadAsync(Stream stream);
}
