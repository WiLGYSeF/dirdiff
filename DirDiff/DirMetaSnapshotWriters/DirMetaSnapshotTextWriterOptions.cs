namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotTextWriterOptions : DirMetaSnapshotWriterOptions
{
    public string Separator { get; set; } = "  ";

    public string NoneValue { get; set; } = "-";
}
