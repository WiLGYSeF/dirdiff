namespace Wilgysef.DirDiff.DirMetaSnapshotWriters;

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

    /// <summary>
    /// Whether to write the file format header.
    /// </summary>
    public bool WriteHeader { get; set; } = true;
}
