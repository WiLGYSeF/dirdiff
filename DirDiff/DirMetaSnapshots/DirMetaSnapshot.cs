namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshot
{
    public string Prefix { get; }

    public IReadOnlyCollection<DirMetaSnapshotEntry> Entries => _entries;

    private readonly List<DirMetaSnapshotEntry> _entries = new();

    internal DirMetaSnapshot(string prefix)
    {
        Prefix = prefix;
    }

    internal void AddEntry(DirMetaSnapshotEntry entry)
    {
        _entries.Add(entry);
    }

    public DirMetaSnapshotDiff Compare(
        DirMetaSnapshot snapshot,
        bool unknownAssumeModified = true,
        TimeSpan? modifyWindow = null)
    {
        // TODO: different prefix

        modifyWindow ??= TimeSpan.Zero;

        var diff = new DirMetaSnapshotDiff();

        var entriesMap = Entries.ToDictionary(e => e.Path);
        var otherEntriesMap = snapshot.Entries.ToDictionary(e => e.Path);

        var hashMap = Entries.Where(e => e.HashHex != null)
            .ToDictionary(e => e.HashHex!);
        var otherHashMap = snapshot.Entries.Where(e => e.HashHex != null)
            .ToDictionary(e => e.HashHex!);

        var otherMovedEntries = new HashSet<DirMetaSnapshotEntry>();

        foreach (var entry in entriesMap.Values)
        {
            if (otherEntriesMap.TryGetValue(entry.Path, out var otherEntry))
            {
                // entry exists in older snapshot

                CompareEntries(entry, otherEntry, diff, false, true, unknownAssumeModified);
            }
            else
            {
                // entry does not exist in older snapshot
                
                if (entry.HashHex != null && otherHashMap.TryGetValue(entry.HashHex, out otherEntry))
                {
                    // entry was moved

                    diff.AddMovedEntry(entry, otherEntry);
                    otherMovedEntries.Add(otherEntry);
                    CompareEntries(entry, otherEntry, diff, true, false, unknownAssumeModified);
                }
                else
                {
                    // entry was created

                    diff.AddCreatedEntry(entry);
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

    private void CompareEntries(
        DirMetaSnapshotEntry entry,
        DirMetaSnapshotEntry other,
        DirMetaSnapshotDiff diff,
        bool changed,
        bool checkModified,
        bool unknownAssumeModified)
    {
        if (entry.Type != other.Type)
        {
            // TODO
            throw new NotImplementedException();
        }

        if (checkModified && !CheckEntryContentsMatch(entry, other).GetValueOrDefault(!unknownAssumeModified))
        {
            diff.AddModifiedEntry(entry, other);
            changed = true;
        }

        if (!CheckEntryTimesMatch(entry, other).GetValueOrDefault(!unknownAssumeModified))
        {
            diff.AddTouchedEntry(entry, other);
            changed = true;
        }

        if (!changed)
        {
            diff.AddUnchangedEntry(entry);
        }
    }

    private static bool? CheckEntryCreationTimeMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.CreatedTime.HasValue && other.CreatedTime.HasValue
            ? entry.CreatedTime.Value == other.CreatedTime.Value
            : null;
    }

    private static bool? CheckEntryLastModifiedTimeMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.LastModifiedTime.HasValue && other.LastModifiedTime.HasValue
            ? entry.LastModifiedTime.Value == other.LastModifiedTime.Value
            : null;
    }

    private static bool? CheckEntryTimesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        var creationTimeMatch = CheckEntryCreationTimeMatch(entry, other);
        if (!creationTimeMatch.HasValue)
        {
            return null;
        }

        var lastModifiedTimeMatch = CheckEntryLastModifiedTimeMatch(entry, other);
        return lastModifiedTimeMatch.HasValue
            ? creationTimeMatch.Value && lastModifiedTimeMatch.Value
            : null;
    }

    private static bool? CheckEntryFileSizesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.FileSize.HasValue && other.FileSize.HasValue
            ? entry.FileSize.Value == other.FileSize.Value
            : null;
    }

    private static bool? CheckEntryHashesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.HashHex != null && other.HashHex != null
            ? entry.HashHex == other.HashHex
            : null;
    }

    private static bool? CheckEntryContentsMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        var hashMatch = CheckEntryHashesMatch(entry, other);
        if (hashMatch.HasValue)
        {
            return hashMatch.Value
                && CheckEntryFileSizesMatch(entry, other).GetValueOrDefault(true);
        }

        var sizeMatch = CheckEntryFileSizesMatch(entry, other);
        var timesMatch = CheckEntryTimesMatch(entry, other);

        return sizeMatch.HasValue && timesMatch.HasValue
            ? sizeMatch.Value && timesMatch.Value
            : null;
    }
}
