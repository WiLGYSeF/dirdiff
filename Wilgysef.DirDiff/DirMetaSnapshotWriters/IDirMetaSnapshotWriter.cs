using Wilgysef.DirDiff.DirMetaSnapshots;

namespace Wilgysef.DirDiff.DirMetaSnapshotWriters;

public interface IDirMetaSnapshotWriter
{
    /// <summary>
    /// Snapshot writer options.
    /// </summary>
    DirMetaSnapshotWriterOptions Options { get; }

    /// <summary>
    /// Configures snapshot writer options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action);

    /// <summary>
    /// Writes snapshot to stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="snapshot">Snapshot.</param>
    /// <returns></returns>
    Task WriteAsync(Stream stream, DirMetaSnapshot snapshot);
}
