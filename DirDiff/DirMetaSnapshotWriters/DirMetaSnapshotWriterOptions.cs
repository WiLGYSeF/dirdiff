namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotWriterOptions
{
    public bool WriteHash { get; set; } = true;

    public bool WriteHashAlgorithm { get; set; } = false;

    public bool WriteCreatedTime { get; set; } = false;

    public bool WriteLastModifiedTime { get; set; } = true;

    public bool WriteFileSize { get; set; } = true;
}
