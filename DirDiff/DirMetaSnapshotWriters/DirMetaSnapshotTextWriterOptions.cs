namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotTextWriterOptions : DirMetaSnapshotWriterOptions
{
    /// <summary>
    /// Separator used to separate columns in the file.
    /// </summary>
    public string Separator { get; set; } = "  ";

    /// <summary>
    /// None value indicator.
    /// </summary>
    public string NoneValue { get; set; } = "-";
}
