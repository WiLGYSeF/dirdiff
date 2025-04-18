﻿namespace Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffWriterOptions
{
    /// <summary>
    /// Directory separator. If <see langword="null"/>, use the respective snapshot directory separators.
    /// </summary>
    public char? DirectorySeparator { get; set; }

    /// <summary>
    /// Replace first snapshot prefixes.
    /// </summary>
    public string? FirstPrefix { get; set; }

    /// <summary>
    /// Replace second snapshot prefixes.
    /// </summary>
    public string? SecondPrefix { get; set; }

    /// <summary>
    /// Whether to write unchanged entries.
    /// </summary>
    public bool WriteUnchanged { get; set; }

    /// <summary>
    /// Whether to write file types.
    /// </summary>
    public bool WriteType { get; set; }
}
