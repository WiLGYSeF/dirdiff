using CommandLine;

namespace DirDiff.Cli.CommandVerbs;

[Verb("diff")]
internal class DiffOptions : ISnapshotReadOptions
{
    [Option("format", Default = "json", HelpText = "Snapshot diff format (bash, powershell, json)")]
    public string? DiffFormat { get; set; }

    [Option("no-size-and-time")]
    public bool NoSizeAndTimeMatch { get; set; }

    [Option("unknown-not-modified")]
    public bool UnknownNotModified { get; set; }

    [Option("time-window", HelpText = "Maximum difference in times before entries are considered different (seconds)")]
    public double? TimeWindow { get; set; }

    [Option("first-prefix", MetaValue = "PREFIX", HelpText = "Replace first snapshot prefix with string")]
    public string? FirstPrefix { get; set; }

    [Option("second-prefix", MetaValue = "PREFIX", HelpText = "Replace second snapshot prefix with string")]
    public string? SecondPrefix { get; set; }

    [Option("hash", HelpText = "Indicates the text snapshot has file hashes")]
    public bool ReadHash { get; set; }

    [Option("last-modified-time", HelpText = "Indicates the text snapshot has file last modified times")]
    public bool ReadLastModifiedTime { get; set; }

    [Option("file-size", HelpText = "Indicates the text snapshot has file sizes")]
    public bool ReadFileSize { get; set; }

    [Value(0)]
    public IEnumerable<string> Arguments { get; set; } = Array.Empty<string>();
}
