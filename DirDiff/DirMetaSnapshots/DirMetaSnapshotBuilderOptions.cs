using DirDiff.Enums;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotBuilderOptions
{
    /// <summary>
    /// Directory separator.
    /// </summary>
    public char DirectorySeparator { get; set; } = Path.DirectorySeparatorChar;

    /// <summary>
    /// Whether to get file sizes.
    /// </summary>
    public bool UseFileSize { get; set; }

    /// <summary>
    /// Whether to get file created times.
    /// </summary>
    public bool UseCreatedTime { get; set; }

    /// <summary>
    /// Wether to get file last modified times.
    /// </summary>
    public bool UseLastModifiedTime { get; set; }

    /// <summary>
    /// Hash algorithm to use to create file hashes.
    /// </summary>
    public HashAlgorithm? HashAlgorithm { get; set; }

    /// <summary>
    /// Maximum difference in times before entries are considered different.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Minimum directory depth limit.
    /// </summary>
    public int? MinDepthLimit { get; set; }

    /// <summary>
    /// Maximum directory depth limit.
    /// </summary>
    public int? MaxDepthLimit { get; set; }

    /// <summary>
    /// Whether to keep the directory order.
    /// </summary>
    public bool KeepDirectoryOrder { get; set; }

    /// <summary>
    /// Throw if files/directories are moved/deleted during snapshot creation.
    /// </summary>
    public bool ThrowIfNotFound { get; set; } = true;
}
