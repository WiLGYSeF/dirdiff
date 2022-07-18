﻿using CommandLine;

namespace DirDiff.Cli.CommandLineOptions;

[Verb("diff")]
internal class DiffOptions
{
    [Option("format", Default = "json", HelpText = "Snapshot diff format (bash, json)")]
    public string? DiffFormat { get; set; }

    [Option("no-size-and-time")]
    public bool NoSizeAndTimeMatch { get; set; }

    [Option("unknown-not-modified")]
    public bool UnknownNotModified { get; set; }

    [Option("modify-window", HelpText = "Maximum difference in times before entries are considered different (seconds)")]
    public double? ModifyWindow { get; set; }

    [Option("hash", HelpText = "Indicates the text snapshot has file hashes")]
    public bool UseHash { get; set; }

    [Option("last-modified-time", HelpText = "Indicates the text snapshot has file last modified times")]
    public bool UseLastModifiedTime { get; set; }

    [Option("file-size", HelpText = "Indicates the text snapshot has file sizes")]
    public bool UseFileSize { get; set; }

    [Value(0)]
    public IEnumerable<string> Arguments { get; set; } = Array.Empty<string>();
}
