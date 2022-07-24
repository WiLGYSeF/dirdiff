﻿using CommandLine;

namespace DirDiff.Cli.CommandVerbs;

[Verb("snapshot", isDefault: true, HelpText = "Creates a file metadata snapshot.")]
internal class SnapshotOptions
{
    [Option("format", MetaValue = "FORMAT", Default = "text", HelpText = "Snapshot format (text, json, yaml)")]
    public string? SnapshotFormat { get; set; }

    [Option("null", HelpText = "Input paths are terminated by a null character instead of a newline")]
    public bool NullSeparatedFilenameInput { get; set; }

    [Option("hash", HelpText = "Calculate file hashes")]
    public bool UseHash { get; set; }

    [Option('m', "last-modified-time", HelpText = "Get file last modified times")]
    public bool UseLastModifiedTime { get; set; }

    [Option("file-size", HelpText = "Get file sizes")]
    public bool UseFileSize { get; set; }

    [Option("remove-prefix", HelpText = "Remove snapshot file prefixes when writing")]
    public bool RemovePrefix { get; set; }

    [Option("time-window", MetaValue = "SECONDS", HelpText = "Maximum difference in times before entries are considered different (seconds)")]
    public double? TimeWindow { get; set; }

    [Option('u', "update", MetaValue = "SNAPSHOT", HelpText = "Use snapshot to create an updated snapshot")]
    public string? UpdateSnapshot { get; set; }

    [Option("update-no-remove", HelpText = "Do not remove non-existing entries when updating a snapshot")]
    public bool UpdateNoRemove { get; set; }

    [Option("update-prefix", MetaValue = "PREFIX", HelpText = "Replace this prefix from entry paths when updating the snapshot")]
    public string? UpdatePrefix { get; set; }

    [Option('o', "output", MetaValue = "FILE", HelpText = "Output filename")]
    public string? OutputFilename { get; set; }

    [Option("output-directory-separator", MetaValue = "SEP", HelpText = "Use this directory separator for snapshot")]
    public char? OutputDirectorySeparator { get; set; }

    [Option('v', "verbose", FlagCounter = true, HelpText = "Verbose mode")]
    public int Verbose { get; set; }

    [Value(0)]
    public ICollection<string> Arguments { get; set; } = Array.Empty<string>();
}
