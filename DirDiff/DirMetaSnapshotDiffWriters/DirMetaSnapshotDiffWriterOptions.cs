namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffWriterOptions
{
    public bool WritePrefix { get; set; } = true;

    public string? FirstPrefix { get; set; }

    public string? SecondPrefix { get; set; }
}
