using CommandLine;

namespace DirDiff.Cli.CommandVerbs;

[Verb("snapshot", isDefault: true, HelpText = "Creates a file metadata snapshot.")]
internal class SnapshotOptions
{
    [Option("format", Default = "text", HelpText = "Snapshot format (text, json)")]
    public string? SnapshotFormat { get; set; }

    [Option("null", HelpText = "Input paths are terminated by a null character instead of a newline")]
    public bool NullSeparatedFilenameInput { get; set; }

    [Option("hash", HelpText = "Calculate file hashes")]
    public bool UseHash { get; set; }

    [Option("last-modified-time", HelpText = "Get file last modified times")]
    public bool UseLastModifiedTime { get; set; }

    [Option("file-size", HelpText = "Get file sizes")]
    public bool UseFileSize { get; set; }

    [Option("remove-prefix", HelpText = "Remove snapshot file prefixes when writing")]
    public bool RemovePrefix { get; set; }

    [Value(0)]
    public IEnumerable<string> Arguments { get; set; } = Array.Empty<string>();
}
