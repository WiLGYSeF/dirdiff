using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public interface IDirMetaSnapshotDiffWriter
{
    DirMetaSnapshotDiffWriterOptions Options {get;}

    IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action);

    Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff);
}
