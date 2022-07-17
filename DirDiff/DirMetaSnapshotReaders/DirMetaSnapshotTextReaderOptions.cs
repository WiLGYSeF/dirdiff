namespace DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotTextReaderOptions : DirMetaSnapshotReaderOptions
{
    public bool ReadGuess { get; set; } = false;

    public bool ReadHash { get; set; } = true;

    public bool ReadHashAlgorithm { get; set; } = false;

    public bool ReadCreatedTime { get; set; } = false;

    public bool ReadLastModifiedTime { get; set; } = true;

    public bool ReadFileSize { get; set; } = true;

    public string Separator { get; set; } = "  ";

    public string NoneValue { get; set; } = "-";
}
