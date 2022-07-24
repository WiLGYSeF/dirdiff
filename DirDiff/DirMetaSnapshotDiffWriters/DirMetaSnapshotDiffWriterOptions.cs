namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffWriterOptions
{
    /// <summary>
    /// Directory separator. If <see langword="null"/>, use the respective snapshot directory separators.
    /// </summary>
    public char? DirectorySeparator { get; set; }

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
