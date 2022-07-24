namespace DirDiff.DirWalkers;

public class DirWalkerOptions
{
    /// <summary>
    /// Whether to return directories.
    /// </summary>
    public bool ReturnDirectories { get; set; }

    /// <summary>
    /// Whether to keep the directory traversal order.
    /// </summary>
    public bool KeepDirectoryOrder { get; set; }

    /// <summary>
    /// Throw if a directory is no longer found.
    /// </summary>
    public bool ThrowIfNotFound { get; set; } = true;

    /// <summary>
    /// Minimum depth limit.
    /// </summary>
    public int? MinDepthLimit { get; set; }

    /// <summary>
    /// Maximum depth limit.
    /// </summary>
    public int? MaxDepthLimit { get; set; }
}
