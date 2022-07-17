using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public interface IDirMetaSnapshotDiffWriter
{
    Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff);
}
