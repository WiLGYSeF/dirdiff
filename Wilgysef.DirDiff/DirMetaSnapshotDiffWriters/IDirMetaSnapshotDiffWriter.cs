using Wilgysef.DirDiff.DirMetaSnapshotComparers;

namespace Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;

public interface IDirMetaSnapshotDiffWriter
{
    /// <summary>
    /// Snapshot diff writer options.
    /// </summary>
    DirMetaSnapshotDiffWriterOptions Options { get; }

    /// <summary>
    /// Configures snapshot diff writer options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action);

    /// <summary>
    /// Writes snapshot diff to stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="diff">Snapshot diff.</param>
    /// <returns></returns>
    Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff);
}
