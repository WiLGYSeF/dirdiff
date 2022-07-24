using CommandLine;

namespace DirDiff.Cli.CommandVerbs;

[Verb("diff")]
internal class DiffOptions
{
    [Option("format", Default = "json", HelpText = "Snapshot diff format (bash, powershell, json, yaml)")]
    public string? DiffFormat { get; set; }

    [Option("no-size-and-time", HelpText = "Do not use file size and last modified time for quick checking entry changes")]
    public bool NoSizeAndTimeMatch { get; set; }

    [Option("unknown-not-modified", HelpText = "Entries with unknown diff status should be treated as not modified")]
    public bool UnknownNotModified { get; set; }

    [Option("time-window", HelpText = "Maximum difference in times before entries are considered different (seconds)")]
    public double? TimeWindow { get; set; }

    [Option("first-prefix", MetaValue = "PREFIX", HelpText = "Replace first snapshot prefix with string")]
    public string? FirstPrefix { get; set; }

    [Option("second-prefix", MetaValue = "PREFIX", HelpText = "Replace second snapshot prefix with string")]
    public string? SecondPrefix { get; set; }

    [Value(0)]
    public ICollection<string> Arguments { get; set; } = Array.Empty<string>();
}
