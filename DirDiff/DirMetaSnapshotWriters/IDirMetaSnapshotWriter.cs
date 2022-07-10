using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotWriters;

public interface IDirMetaSnapshotWriter
{
    Task Write(Stream stream, DirMetaSnapshot snapshot);
}
