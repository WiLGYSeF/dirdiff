namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotJsonWriterOptions : DirMetaSnapshotWriterOptions
{
    public bool UseUnixTimestamp { get; set; }

    public bool WriteIndented { get; set; }
}
