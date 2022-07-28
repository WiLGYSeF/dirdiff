using Wilgysef.DirDiff.DirMetaSnapshots;

namespace Wilgysef.DirDiff.DirMetaSnapshotReaders;

public interface IDirMetaSnapshotReader
{
    /// <summary>
    /// Snapshot reader options.
    /// </summary>
    DirMetaSnapshotReaderOptions Options { get; }

    /// <summary>
    /// Configure snapshot reader options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action);

    /// <summary>
    /// Reads snapshot from stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <returns>Snapshot.</returns>
    Task<DirMetaSnapshot> ReadAsync(Stream stream);
}
