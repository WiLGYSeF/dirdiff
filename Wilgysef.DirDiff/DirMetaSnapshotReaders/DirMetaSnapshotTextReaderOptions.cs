namespace Wilgysef.DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotTextReaderOptions : DirMetaSnapshotReaderOptions
{
    /// <summary>
    /// Whether to guess the file format. Ignores all other Read options if true.
    /// </summary>
    public bool ReadGuess { get; set; } = false;

    /// <summary>
    /// Whether hashes are present in the file.
    /// </summary>
    public bool ReadHash { get; set; } = true;

    /// <summary>
    /// Whether hash algorithms are present in the file.
    /// </summary>
    public bool ReadHashAlgorithm { get; set; } = false;

    /// <summary>
    /// Whether created times are present in the file.
    /// </summary>
    public bool ReadCreatedTime { get; set; } = false;

    /// <summary>
    /// Whether last modified times are present in the file.
    /// </summary>
    public bool ReadLastModifiedTime { get; set; } = true;

    /// <summary>
    /// Whether file sizes are present in the file.
    /// </summary>
    public bool ReadFileSize { get; set; } = true;

    /// <summary>
    /// Separator used to separate columns in the file.
    /// </summary>
    public string Separator { get; set; } = "  ";

    /// <summary>
    /// None value indicator.
    /// </summary>
    public string NoneValue { get; set; } = "-";

    internal DirMetaSnapshotTextReaderOptions Copy()
    {
        return new DirMetaSnapshotTextReaderOptions
        {
            ReadGuess = ReadGuess,
            ReadHash = ReadHash,
            ReadHashAlgorithm = ReadHashAlgorithm,
            ReadCreatedTime = ReadCreatedTime,
            ReadLastModifiedTime = ReadLastModifiedTime,
            ReadFileSize = ReadFileSize,
            Separator = Separator,
            NoneValue = NoneValue,
        };
    }

    internal void DisableReadOptions()
    {
        ReadGuess = false;
        ReadHash = false;
        ReadHashAlgorithm = false;
        ReadCreatedTime = false;
        ReadLastModifiedTime = false;
        ReadFileSize = false;
    }
}
