using DirDiff.Enums;
using DirDiff.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DirDiff.DirMetaSnapshots;

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
        window ??= TimeSpan.Zero;

        var diff = new DirMetaSnapshotDiff(snapshot, this);

        var entriesMap = Entries.ToDictionary(e => PathWithoutPrefix(e.Path));
        var otherEntriesMap = snapshot.Entries.ToDictionary(e => snapshot.PathWithoutPrefix(e.Path));

        var otherHashMap = CreateHashMap(snapshot.Entries);

        var otherSizeMap = CreateSizeMap(snapshot.Entries);

        var otherMovedEntries = new HashSet<DirMetaSnapshotEntry>();

        foreach (var entry in entriesMap.Values)
        {
            var entryPathWithoutPrefix = PathWithoutPrefix(entry.Path);

            if (otherEntriesMap.TryGetValue(entryPathWithoutPrefix, out var otherEntry))
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

                if (entry.HashHex != null && otherHashMap.TryGetValue(entry.HashHex, out var otherHashEntries))
                {
                    // entry matches hash of otherEntry

                    var moved = false;

                    otherEntry = otherHashEntries.FirstOrDefault(e => e.LastModifiedTime == entry.LastModifiedTime);
                    if (otherEntry != null)
                    {
                        moved = !entriesMap.ContainsKey(snapshot.PathWithoutPrefix(otherEntry.Path)) && !otherMovedEntries.Contains(otherEntry);
                    }
                    else
                    {
                        otherEntry = otherHashEntries.FirstOrDefault(e => !entriesMap.ContainsKey(snapshot.PathWithoutPrefix(e.Path)) && !otherMovedEntries.Contains(e));
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
            if (!entriesMap.TryGetValue(snapshot.PathWithoutPrefix(otherEntry.Path), out var entry))
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

        // don't care about created times
        if (!CheckEntryLastModifiedTimeMatch(entry, other, window).GetValueOrDefault(!unknownAssumeModified))
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
    /// Creates a map of file sizes to entries.
    /// </summary>
    /// <param name="entries">Entries.</param>
    /// <returns>File size map.</returns>
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
    /// Creates a map of file hashes to entries.
    /// </summary>
    /// <param name="entries">Entries.</param>
    /// <returns>File hash map.</returns>
    private static Dictionary<string, List<DirMetaSnapshotEntry>> CreateHashMap(IEnumerable<DirMetaSnapshotEntry> entries)
    {
        var dict = new Dictionary<string, List<DirMetaSnapshotEntry>>();

        foreach (var entry in entries)
        {
            if (entry.HashHex == null)
            {
                continue;
            }

            if (!dict.TryGetValue(entry.HashHex, out var hashEntries))
            {
                hashEntries = new List<DirMetaSnapshotEntry>();
                dict[entry.HashHex] = hashEntries;
            }
            hashEntries.Add(entry);
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
    private DirMetaSnapshotEntry? GetMovedEntryFromSizeMap(
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

        var possibleEntries = sizeEntries
            .Where(entry => entry.Type != FileType.Directory
                && entry.LastModifiedTime.HasValue
                && (entry.LastModifiedTime.Value - lastModifiedTime).Duration() <= window
                && !entries.ContainsKey(PathWithoutPrefix(entry.Path)))
            .ToList();
        return possibleEntries.Count == 1 ? possibleEntries[0] : null;
    }

    private string GetCommonPrefix(string path)
    {
        if (Prefix == null)
        {
            return GetDirectoryParts(path)[..^1].Join(DirectorySeparator);
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
        return path.Split(DirectorySeparator);
    }
}
