namespace DirDiff.Cli.CommandVerbs;

internal interface ISnapshotReadOptions
{
    public bool ReadHash { get; set; }

    public bool ReadLastModifiedTime { get; set; }

    public bool ReadFileSize { get; set; }
}
