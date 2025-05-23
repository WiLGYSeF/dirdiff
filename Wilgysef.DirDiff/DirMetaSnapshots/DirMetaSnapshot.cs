﻿using Wilgysef.DirDiff.DirMetaSnapshotComparers;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Wilgysef.DirDiff.Utilities;

namespace Wilgysef.DirDiff.DirMetaSnapshots;

public class DirMetaSnapshot
{
    /// <summary>
    /// Common path prefix.
    /// </summary>
    public string? Prefix
    {
        get => _prefix;
        private set
        {
            _prefix = value;
            _prefixParts = _prefix != null ? GetDirectoryParts(_prefix) : null;
        }
    }

    /// <summary>
    /// Directory separator.
    /// </summary>
    public char DirectorySeparator { get; }

    /// <summary>
    /// File entries in snapshot.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotEntry> Entries => _entries.Values;

    private readonly Dictionary<string, DirMetaSnapshotEntry> _entries = new();

    private string? _prefix;
    private string[]? _prefixParts;

    public DirMetaSnapshot() : this(Path.DirectorySeparatorChar) { }

    public DirMetaSnapshot(char directorySeparator)
    {
        DirectorySeparator = directorySeparator;
    }

    /// <summary>
    /// Checks if the snapshot contains the entry path.
    /// </summary>
    /// <param name="path">Entry path.</param>
    /// <returns><see langword="true"/> if the entry path is in the snapshot, otherwise <see langword="false"/>.</returns>
    public bool ContainsPath(string path)
    {
        return _entries.ContainsKey(path);
    }

    public void AddEntry(DirMetaSnapshotEntry entry)
    {
        _entries.Add(entry.Path, entry);
        Prefix = GetCommonPrefix(entry.Path);
    }

    public DirMetaSnapshotEntry GetEntry(string path)
    {
        return _entries[path];
    }

    public bool TryGetEntry(string path, [MaybeNullWhen(false)] out DirMetaSnapshotEntry entry)
    {
        if (_entries.TryGetValue(path, out entry))
        {
            return true;
        }

        entry = default;
        return false;
    }

    /// <summary>
    /// Gets the path without snapshot prefix.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns>Path without snapshot prefix.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> does not start with snapshot prefix.</exception>
    public string PathWithoutPrefix(string path)
    {
        if (Prefix == null || Prefix.Length == 0)
        {
            return path;
        }

        if (!path.StartsWith(Prefix))
        {
            throw new ArgumentException("Path does not start with expected prefix.", nameof(path));
        }
        return path[Prefix.Length..];
    }

    /// <summary>
    /// Change the snapshot path's directory separator.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="directorySeparator">Directory separator to change to.</param>
    /// <returns>Snapshot path with specified directory separator.</returns>
    public string ChangePathDirectorySeparator(string path, char directorySeparator)
    {
        return directorySeparator != DirectorySeparator
            ? PathUtils.ChangePathDirectorySeparator(path, DirectorySeparator, directorySeparator)
            : path;
    }

    private string GetCommonPrefix(string path)
    {
        if (Prefix == null)
        {
            var prefix = string.Join(DirectorySeparator, GetDirectoryParts(path)[..^1]);
            if (prefix.Length > 0)
            {
                prefix += DirectorySeparator;
            }
            return prefix;
        }

        if (Prefix.Length == 0 || path.StartsWith(Prefix))
        {
            return Prefix;
        }

        var pathParts = GetDirectoryParts(path);

        var builder = new StringBuilder();
        var length = Math.Min(_prefixParts!.Length, pathParts.Length);
        var index = 0;
        for (; index < length; index++)
        {
            if (_prefixParts[index] != pathParts[index])
            {
                break;
            }
            builder.Append(_prefixParts[index]);
            builder.Append(DirectorySeparator);
        }

        return builder.ToString();
    }

    private string[] GetDirectoryParts(string path)
    {
        return PathUtils.GetDirectoryParts(path, DirectorySeparator);
    }
}
