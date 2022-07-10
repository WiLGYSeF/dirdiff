﻿namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshot
{
    /// <summary>
    /// Common path prefix.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// File entries in snapshot.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotEntry> Entries => _entries.Values;

    private readonly Dictionary<string, DirMetaSnapshotEntry> _entries = new();

    public DirMetaSnapshot()
    {
        Prefix = "";
    }

    internal DirMetaSnapshot(string prefix)
    {
        Prefix = prefix;
    }

    internal void AddEntry(DirMetaSnapshotEntry entry)
    {
        _entries.Add(entry.Path, entry);
    }

    /// <summary>
    /// Creates a diff between two snapshots, where the snapshot calling compare is considered more recent.
    /// </summary>
    /// <param name="snapshot">Snapshot to compare, considered less recent than the calling snapshot.</param>
    /// <param name="sizeAndTimeMatch">
    /// Indicates if matching entry last modified times and file sizes can be considered a match if there are no entry hashes.
    /// Otherwise, they are considered unknown.
    /// </param>
    /// <param name="unknownAssumeModified">Indicates if unknown entry comparisons should be treated as modifications.</param>
    /// <param name="window">Maximum difference in times before entries are considered different. Defaults to zero.</param>
    /// <returns>Snapshot diff.</returns>
    public DirMetaSnapshotDiff Compare(
        DirMetaSnapshot snapshot,
        bool sizeAndTimeMatch = true,
        bool unknownAssumeModified = true,
        TimeSpan? window = null)
    {
        // TODO: different prefix

        window ??= TimeSpan.Zero;

        var diff = new DirMetaSnapshotDiff();

        var entriesMap = Entries.ToDictionary(e => e.Path);
        var otherEntriesMap = snapshot.Entries.ToDictionary(e => e.Path);

        var hashMap = Entries.Where(e => e.HashHex != null)
            .ToDictionary(e => e.HashHex!);
        var otherHashMap = snapshot.Entries.Where(e => e.HashHex != null)
            .ToDictionary(e => e.HashHex!);

        var otherSizeMap = CreateSizeMap(snapshot.Entries);

        var otherMovedEntries = new HashSet<DirMetaSnapshotEntry>();

        foreach (var entry in entriesMap.Values)
        {
            if (otherEntriesMap.TryGetValue(entry.Path, out var otherEntry))
            {
                // entry exists in older snapshot

                CompareEntries(
                    entry,
                    otherEntry,
                    diff,
                    false,
                    true,
                    window.Value,
                    sizeAndTimeMatch,
                    unknownAssumeModified);
            }
            else
            {
                // entry does not exist in older snapshot

                if (entry.HashHex != null && otherHashMap.TryGetValue(entry.HashHex, out otherEntry))
                {
                    // entry was moved

                    diff.AddMovedEntry(entry, otherEntry);
                    otherMovedEntries.Add(otherEntry);
                    CompareEntries(
                        entry,
                        otherEntry,
                        diff,
                        true,
                        false,
                        window.Value,
                        sizeAndTimeMatch,
                        unknownAssumeModified);
                }
                else
                {
                    var moved = false;
                    if (sizeAndTimeMatch
                        && entry.Hash == null
                        && entry.FileSize.HasValue
                        && entry.LastModifiedTime.HasValue)
                    {
                        // check if an entry was moved by the size and last modified time

                        var movedEntry = GetMovedEntryFromSizeMap(
                            otherSizeMap,
                            _entries,
                            entry.FileSize.Value,
                            entry.LastModifiedTime.Value,
                            window.Value);
                        if (movedEntry != null)
                        {
                            // entry was moved based on size and last modified time

                            diff.AddMovedEntry(entry, movedEntry);
                            otherMovedEntries.Add(movedEntry);
                            moved = true;
                        }
                    }

                    if (!moved)
                    {
                        // entry was created

                        diff.AddCreatedEntry(entry);
                    }
                }
            }
        }

        foreach (var otherEntry in otherEntriesMap.Values)
        {
            if (!entriesMap.TryGetValue(otherEntry.Path, out var entry))
            {
                // entry does not exist in newer snapshot

                if (!otherMovedEntries.Contains(otherEntry))
                {
                    // entry does not exist in newer snapshot and was not moved

                    diff.AddDeletedEntry(otherEntry);
                }
            }

            // other cases have already been handled
        }

        return diff;
    }

    /// <summary>
    /// Compare entries and add them to the diff.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <param name="diff">Diff.</param>
    /// <param name="changed">Indicates if the entry was marked as changed.</param>
    /// <param name="checkModified">Indicates if it should bother checking if entries have been modified.</param>
    /// <param name="window">Maximum difference in times before entries are considered different. Defaults to zero.</param>
    /// <param name="sizeAndTimeMatch">
    /// Indicates if matching entry last modified times and file sizes can be considered a match if there are no entry hashes.
    /// Otherwise, they are considered unknown.
    /// </param>
    /// <param name="unknownAssumeModified">Indicates if unknown entry comparisons should be treated as modifications.</param>
    private void CompareEntries(
        DirMetaSnapshotEntry entry,
        DirMetaSnapshotEntry other,
        DirMetaSnapshotDiff diff,
        bool changed,
        bool checkModified,
        TimeSpan window,
        bool sizeAndTimeMatch,
        bool unknownAssumeModified)
    {
        if (entry.Type != other.Type)
        {
            diff.AddCreatedEntry(entry);
            diff.AddDeletedEntry(other);
            changed = true;
        }

        if (checkModified
            && !CheckEntryContentsMatch(entry, other, window, sizeAndTimeMatch)
                .GetValueOrDefault(!unknownAssumeModified))
        {
            diff.AddModifiedEntry(entry, other);
            changed = true;
        }

        if (!CheckEntryTimesMatch(entry, other, window).GetValueOrDefault(!unknownAssumeModified))
        {
            diff.AddTouchedEntry(entry, other);
            changed = true;
        }

        if (!changed)
        {
            diff.AddUnchangedEntry(entry);
        }
    }

    /// <summary>
    /// Check if the entry creation times match.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <param name="window">Maximum difference in times before entries are considered different. Defaults to zero.</param>
    /// <returns>Returns true if entry creation times match, false otherwise. If entry creation times are not present, return null.</returns>
    private static bool? CheckEntryCreationTimeMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other, TimeSpan window)
    {
        return entry.CreatedTime.HasValue && other.CreatedTime.HasValue
            ? (entry.CreatedTime.Value - other.CreatedTime.Value).Duration() <= window
            : null;
    }

    /// <summary>
    /// Check if the entry last modified times match.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <param name="window">Maximum difference in times before entries are considered different. Defaults to zero.</param>
    /// <returns>Returns true if entry last modified times match, false otherwise. If entry last modified times are not present, return null.</returns>
    private static bool? CheckEntryLastModifiedTimeMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other, TimeSpan window)
    {
        return entry.LastModifiedTime.HasValue && other.LastModifiedTime.HasValue
            ? (entry.LastModifiedTime.Value - other.LastModifiedTime.Value).Duration() <= window
            : null;
    }

    /// <summary>
    /// Check if the entry creation and last modified times match.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <param name="window">Maximum difference in times before entries are considered different. Defaults to zero.</param>
    /// <returns>Returns true if entry times match, false otherwise. If entry times are not present, return null.</returns>
    private static bool? CheckEntryTimesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other, TimeSpan window)
    {
        var creationTimeMatch = CheckEntryCreationTimeMatch(entry, other, window);
        if (!creationTimeMatch.HasValue)
        {
            return null;
        }

        var lastModifiedTimeMatch = CheckEntryLastModifiedTimeMatch(entry, other, window);
        return lastModifiedTimeMatch.HasValue
            ? creationTimeMatch.Value && lastModifiedTimeMatch.Value
            : null;
    }

    /// <summary>
    /// Check if the entry file sizes match.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <returns>Returns true if entry file sizes match, false otherwise. If entry file sizes are not present, return null.</returns>
    private static bool? CheckEntryFileSizesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.FileSize.HasValue && other.FileSize.HasValue
            ? entry.FileSize.Value == other.FileSize.Value
            : null;
    }

    /// <summary>
    /// Check if the entry hashes match.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <returns>Returns true if entry hashes match, false otherwise. If entry hashes are not present, return null.</returns>
    private static bool? CheckEntryHashesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.HashHex != null && other.HashHex != null
            ? entry.HashHex == other.HashHex
            : null;
    }

    /// <summary>
    /// Check if entry contents match. Checks entry hashes, or the file sizes and entry times match if enabled.
    /// </summary>
    /// <param name="entry">Entry, more recent.</param>
    /// <param name="other">Other entry, less recent.</param>
    /// <param name="window">Maximum difference in times before entries are considered different. Defaults to zero.</param>
    /// <param name="sizeAndTimeMatch">
    /// Indicates if matching entry last modified times and file sizes can be considered a match if there are no entry hashes.
    /// Otherwise, they are considered unknown.
    /// </param>
    /// <returns>Returns true if the entry contents match, false otherwise. If entry contents are not present, return null.</returns>
    private static bool? CheckEntryContentsMatch(
        DirMetaSnapshotEntry entry,
        DirMetaSnapshotEntry other,
        TimeSpan window,
        bool sizeAndTimeMatch)
    {
        var hashMatch = CheckEntryHashesMatch(entry, other);
        if (hashMatch.HasValue)
        {
            return hashMatch.Value
                && CheckEntryFileSizesMatch(entry, other).GetValueOrDefault(true);
        }

        var sizeMatch = CheckEntryFileSizesMatch(entry, other);
        var timesMatch = CheckEntryTimesMatch(entry, other, window);

        if (!sizeMatch.HasValue || !timesMatch.HasValue)
        {
            return null;
        }
        if (!sizeMatch.Value || !timesMatch.Value)
        {
            return false;
        }

        return sizeAndTimeMatch ? true : null;
    }

    private static Dictionary<long, List<DirMetaSnapshotEntry>> CreateSizeMap(IEnumerable<DirMetaSnapshotEntry> entries)
    {
        var dict = new Dictionary<long, List<DirMetaSnapshotEntry>>();

        foreach (var entry in entries)
        {
            if (!entry.FileSize.HasValue)
            {
                continue;
            }

            if (!dict.TryGetValue(entry.FileSize.Value, out var dictEntries))
            {
                dictEntries = new List<DirMetaSnapshotEntry>();
                dict.Add(entry.FileSize.Value, dictEntries);
            }
            dictEntries.Add(entry);
        }

        return dict;
    }

    /// <summary>
    /// Gets the moved entry by the file size and last modified time if it does not exist in the recent snapshot entries.
    /// </summary>
    /// <param name="sizeMap"></param>
    /// <param name="entries"></param>
    /// <param name="fileSize"></param>
    /// <param name="lastModifiedTime"></param>
    /// <param name="window"></param>
    /// <returns></returns>
    private static DirMetaSnapshotEntry? GetMovedEntryFromSizeMap(
        IReadOnlyDictionary<long, List<DirMetaSnapshotEntry>> sizeMap,
        IReadOnlyDictionary<string, DirMetaSnapshotEntry> entries,
        long fileSize,
        DateTime lastModifiedTime,
        TimeSpan window)
    {
        if (!sizeMap.TryGetValue(fileSize, out var sizeEntries))
        {
            return null;
        }

        var possibleEntries = sizeEntries.Where(entry =>
            entry.LastModifiedTime.HasValue
            && (entry.LastModifiedTime.Value - lastModifiedTime).Duration() <= window
            && !entries.ContainsKey(entry.Path)).ToList();
        return possibleEntries.Count == 1 ? possibleEntries.First() : null;
    }
}
