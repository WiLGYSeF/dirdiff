namespace DirDiff.DirWalkers;

internal class DirWalkerOptions
{
    public bool ReturnDirectories { get; set; }

    public bool KeepDirectoryOrder { get; set; }

    public bool ThrowIfNotFound { get; set; } = true;

    public int? MinDepthLimit { get; set; }

    public int? MaxDepthLimit { get; set; }
}
