namespace Wilgysef.DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotWriterOptions
{
    /// <summary>
    /// Whether to write snapshot file prefixes.
    /// </summary>
    public bool WritePrefix { get; set; } = true;

    /// <summary>
    /// Directory separator to write paths with.
    /// </summary>
    public char? DirectorySeparator { get; set; }

    /// <summary>
    /// Whether to sort by path.
    /// </summary>
    public bool SortByPath { get; set; } = false;

    /// <summary>
    /// Whether to write file hashes.
    /// </summary>
    public bool WriteHash { get; set; } = true;

    /// <summary>
    /// Whether to write file hash algorithms.
    /// </summary>
    public bool WriteHashAlgorithm { get; set; } = false;

    /// <summary>
    /// Whether to write file created times.
    /// </summary>
    public bool WriteCreatedTime { get; set; } = false;

    /// <summary>
    /// Whether to write file last modified times.
    /// </summary>
    public bool WriteLastModifiedTime { get; set; } = true;

    /// <summary>
    /// Whether to write file sizes.
    /// </summary>
    public bool WriteFileSize { get; set; } = true;
}
