using CommandLine;

namespace Wilgysef.DirDiff.Cli.CommandVerbs;

[Verb("diff")]
internal class DiffOptions
{
    [Option("no-size-and-time", HelpText = "Do not use file size and last modified time for quick checking entry changes")]
    public bool NoSizeAndTimeMatch { get; set; }

    [Option('u', "unknown-not-modified", HelpText = "Entries with unknown diff status should be treated as not modified")]
    public bool UnknownNotModified { get; set; }

    [Option("time-window", MetaValue = "SECONDS", HelpText = "Maximum difference in times before entries are considered different (seconds)")]
    public double? TimeWindow { get; set; }

    [Option("first-prefix", MetaValue = "PREFIX", HelpText = "Replace first snapshot prefix with string")]
    public string? FirstPrefix { get; set; }

    [Option("second-prefix", MetaValue = "PREFIX", HelpText = "Replace second snapshot prefix with string")]
    public string? SecondPrefix { get; set; }

    [Option('o', "output", MetaValue = "FILE", HelpText = "Output filename")]
    public string? OutputFilename { get; set; }

    [Option('f', "format", MetaValue = "FORMAT", Default = "json", HelpText = "Snapshot diff format, defaults to JSON (bash, powershell, json, yaml)")]
    public string? DiffFormat { get; set; }

    [Option('v', "verbose", FlagCounter = true, HelpText = "Verbose mode")]
    public int Verbose { get; set; }

    [Value(0)]
    public ICollection<string> Arguments { get; set; } = Array.Empty<string>();
}
