namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffWriterOptions
{
    /// <summary>
    /// Whether to write snapshot file prefixes.
    /// </summary>
    public bool WritePrefix { get; set; } = true;

    /// <summary>
    /// Replace first snapshot prefixes.
    /// </summary>
    public string? FirstPrefix { get; set; }

    /// <summary>
    /// Replace second snapshot prefixes.
    /// </summary>
    public string? SecondPrefix { get; set; }
}
