namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotJsonWriterOptions : DirMetaSnapshotWriterOptions
{
    /// <summary>
    /// Whether to use unix timestamps or ISO8601 for file created times and last modified times.
    /// </summary>
    public bool UseUnixTimestamp { get; set; }

    /// <summary>
    /// Whether to indent JSON.
    /// </summary>
    public bool WriteIndented { get; set; }
}
