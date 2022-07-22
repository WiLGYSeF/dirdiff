using CommandLine;

namespace DirDiff.Cli.CommandVerbs;

[Verb("snapshot", isDefault: true, HelpText = "Creates a file metadata snapshot.")]
internal class SnapshotOptions : ISnapshotReadOptions
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

    [Option("update", MetaValue = "SNAPSHOT", HelpText = "Use snapshot to create an updated snapshot")]
    public string? UpdateSnapshot { get; set; }

    [Option("read-hash", HelpText = "Indicates the text snapshot has file hashes")]
    public bool ReadHash { get; set; }

    [Option("read-last-modified-time", HelpText = "Indicates the text snapshot has file last modified times")]
    public bool ReadLastModifiedTime { get; set; }

    [Option("read-file-size", HelpText = "Indicates the text snapshot has file sizes")]
    public bool ReadFileSize { get; set; }

    [Option("time-window", HelpText = "Maximum difference in times before entries are considered different (seconds)")]
    public double? TimeWindow { get; set; }

    [Value(0)]
    public IEnumerable<string> Arguments { get; set; } = Array.Empty<string>();
}
