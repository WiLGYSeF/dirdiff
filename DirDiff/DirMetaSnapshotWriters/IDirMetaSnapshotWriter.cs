using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotWriters;

public interface IDirMetaSnapshotWriter
{
    DirMetaSnapshotWriterOptions Options { get; }

    IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action);

    Task Write(Stream stream, DirMetaSnapshot snapshot);
}
