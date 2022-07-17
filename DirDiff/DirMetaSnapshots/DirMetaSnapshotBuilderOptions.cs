using DirDiff.Enums;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotBuilderOptions
{
    public char DirectorySeparator { get; set; } = Path.DirectorySeparatorChar;

    public bool UseFileSize { get; set; }

    public bool UseCreatedTime { get; set; }

    public bool UseLastModifiedTime { get; set; }

    public HashAlgorithm? HashAlgorithm { get; set; }

    public int? MinDepthLimit { get; set; }

    public int? MaxDepthLimit { get; set; }

    public bool KeepDirectoryOrder { get; set; }

    public bool ThrowIfNotFound { get; set; } = true;
}
