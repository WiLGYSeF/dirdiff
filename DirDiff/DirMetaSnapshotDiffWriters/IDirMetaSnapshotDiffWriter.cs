using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public interface IDirMetaSnapshotDiffWriter
{
    Task Write(Stream stream, DirMetaSnapshotDiff diff);
}
