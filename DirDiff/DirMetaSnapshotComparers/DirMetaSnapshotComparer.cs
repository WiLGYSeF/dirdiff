using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirDiff.DirMetaSnapshotComparers;

public class DirMetaSnapshotComparer : IDirMetaSnapshotComparer
{
    public DirMetaSnapshotComparerOptions Options { get; } = new();

    public IDirMetaSnapshotComparer Configure(Action<DirMetaSnapshotComparerOptions> action)
    {
        action(Options);
        return this;
    }

    public DirMetaSnapshotDiff Compare(DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        var diff = new DirMetaSnapshotDiff(firstSnapshot, secondSnapshot);

        var entriesMap = secondSnapshot.Entries.ToDictionary(e => secondSnapshot.PathWithoutPrefix(e.Path));
        var otherEntriesMap = firstSnapshot.Entries.ToDictionary(
            e => firstSnapshot.ChangePathDirectorySeparator(
                firstSnapshot.PathWithoutPrefix(e.Path),
                secondSnapshot.DirectorySeparator));

        CreateMaps(firstSnapshot.Entries, out var otherSizeMap, out var otherHashMap);

        var otherMovedEntries = new HashSet<DirMetaSnapshotEntry>();

        foreach (var entry in entriesMap.Values)
        {
            var entryPathWithoutPrefix = secondSnapshot.PathWithoutPrefix(entry.Path);

            if (otherEntriesMap.TryGetValue(entryPathWithoutPrefix, out var otherEntry))
            {
                // entry exists in older snapshot

                CompareEntries(
                    entry,
                    otherEntry,
                    diff,
                    false,
                    true);
            }
            else
            {
                // entry does not exist in older snapshot

                if (entry.HashHex != null && otherHashMap.TryGetValue(entry.HashHex, out var otherHashEntries))
                {
                    // entry matches hash of otherEntry

                    var moved = false;

                    otherEntry = otherHashEntries.FirstOrDefault(e => e.LastModifiedTime == entry.LastModifiedTime);

                    var movedPredicate = (DirMetaSnapshotEntry e) => !entriesMap.ContainsKey(
                        firstSnapshot.ChangePathDirectorySeparator(
                            firstSnapshot.PathWithoutPrefix(e.Path),
                            secondSnapshot.DirectorySeparator))
                        && !otherMovedEntries.Contains(e);

                    if (otherEntry != null)
                    {
                        moved = movedPredicate(otherEntry);
                    }
                    else
                    {
                        otherEntry = otherHashEntries.FirstOrDefault(movedPredicate);
                        if (otherEntry != null)
                        {
                            moved = true;
                        }
                        else
                        {
                            otherEntry = otherHashEntries[0];
                        }
                    }

                    if (moved)
                    {
                        // entry was moved

                        diff.AddMovedEntry(entry, otherEntry);
                        otherMovedEntries.Add(otherEntry);
                    }
                    else
                    {
                        // entry was copied

                        diff.AddCopiedEntry(entry, otherEntry);
                    }

                    CompareEntries(
                        entry,
                        otherEntry,
                        diff,
                        true,
                        false);
                }
                else
                {
                    var moved = false;
                    if (Options.SizeAndTimeMatch
                        && entry.Hash == null
                        && entry.FileSize.HasValue
                        && entry.LastModifiedTime.HasValue)
                    {
                        // check if an entry was moved by the size and last modified time

                        var movedEntry = GetMovedEntryFromSizeMap(
                            firstSnapshot,
                            secondSnapshot,
                            otherSizeMap,
                            otherMovedEntries,
                            entry.FileSize.Value,
                            entry.LastModifiedTime.Value);
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
            if (!entriesMap.TryGetValue(
                firstSnapshot.ChangePathDirectorySeparator(
                    firstSnapshot.PathWithoutPrefix(otherEntry.Path),
                    secondSnapshot.DirectorySeparator),
                out var entry))
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
    private void CompareEntries(
        DirMetaSnapshotEntry entry,
        DirMetaSnapshotEntry other,
        DirMetaSnapshotDiff diff,
        bool changed,
        bool checkModified)
    {
        if (entry.Type != other.Type)
        {
            diff.AddCreatedEntry(entry);
            diff.AddDeletedEntry(other);
            changed = true;
        }

        if (checkModified
            && !CheckEntryContentsMatch(entry, other, Options.TimeWindow, Options.SizeAndTimeMatch)
                .GetValueOrDefault(!Options.UnknownAssumeModified))
        {
            diff.AddModifiedEntry(entry, other);
            changed = true;
        }

        // don't care about created times
        if (!CheckEntryLastModifiedTimeMatch(entry, other, Options.TimeWindow).GetValueOrDefault(!Options.UnknownAssumeModified))
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
            ? entry.CreatedTime.Value.Within(other.CreatedTime.Value, window)
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
            ? entry.LastModifiedTime.Value.Within(other.LastModifiedTime.Value, window)
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
        return entry.Hash != null && other.Hash != null
            ? entry.Hash.SequenceEqual(other.Hash)
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
        // don't care about created times
        var timesMatch = CheckEntryLastModifiedTimeMatch(entry, other, window);

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

    /// <summary>
    /// Creates entry maps.
    /// </summary>
    /// <param name="entries">Entries.</param>
    /// <param name="sizeMap">File size map.</param>
    /// <param name="hashMap">File hash map.</param>
    /// <returns></returns>
    private static void CreateMaps(
        IEnumerable<DirMetaSnapshotEntry> entries,
        out Dictionary<long, List<DirMetaSnapshotEntry>> sizeMap,
        out Dictionary<string, List<DirMetaSnapshotEntry>> hashMap)
    {
        sizeMap = new Dictionary<long, List<DirMetaSnapshotEntry>>();
        hashMap = new Dictionary<string, List<DirMetaSnapshotEntry>>();

        foreach (var entry in entries)
        {
            if (entry.FileSize.HasValue)
            {
                if (!sizeMap.TryGetValue(entry.FileSize.Value, out var sizeEntries))
                {
                    sizeEntries = new List<DirMetaSnapshotEntry>();
                    sizeMap.Add(entry.FileSize.Value, sizeEntries);
                }
                sizeEntries.Add(entry);
            }

            if (entry.HashHex != null)
            {
                if (!hashMap.TryGetValue(entry.HashHex, out var hashEntries))
                {
                    hashEntries = new List<DirMetaSnapshotEntry>();
                    hashMap[entry.HashHex] = hashEntries;
                }
                hashEntries.Add(entry);
            }
        }
    }

    /// <summary>
    /// Gets the moved entry by the file size and last modified time.
    /// The entry cannot already exist in the second snapshot and cannot have already been moved.
    /// </summary>
    /// <param name="firstSnapshot">First snapshot.</param>
    /// <param name="secondSnapshot">Second snapshot.</param>
    /// <param name="otherSizeMap">First snapshot entry file size map.</param>
    /// <param name="otherMovedEntries">First snapshot entries already moved.</param>
    /// <param name="fileSize">File size.</param>
    /// <param name="lastModifiedTime">File last modified time.</param>
    /// <returns>Probable moved entry candidate.</returns>
    private DirMetaSnapshotEntry? GetMovedEntryFromSizeMap(
        DirMetaSnapshot firstSnapshot,
        DirMetaSnapshot secondSnapshot,
        IReadOnlyDictionary<long, List<DirMetaSnapshotEntry>> otherSizeMap,
        IReadOnlyCollection<DirMetaSnapshotEntry> otherMovedEntries,
        long fileSize,
        DateTime lastModifiedTime)
    {
        if (!otherSizeMap.TryGetValue(fileSize, out var sizeEntries))
        {
            return null;
        }

        var possibleEntries = sizeEntries
            .Where(entry => entry.Type != FileType.Directory
                && entry.LastModifiedTime.HasValue
                && entry.LastModifiedTime.Value.Within(lastModifiedTime, Options.TimeWindow)
                && !otherMovedEntries.Contains(entry)
                && !secondSnapshot.ContainsPath(secondSnapshot.Prefix + firstSnapshot.PathWithoutPrefix(entry.Path)))
            .ToList();
        return possibleEntries.Count == 1 ? possibleEntries[0] : null;
    }
}
