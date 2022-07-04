namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshot
{
    public IReadOnlyCollection<DirMetaSnapshotEntry> Entries => _entries;

    private readonly List<DirMetaSnapshotEntry> _entries = new();

    internal DirMetaSnapshot() { }

    internal void AddEntry(DirMetaSnapshotEntry entry)
    {
        _entries.Add(entry);
    }

    public DirMetaSnapshotDiff Compare(DirMetaSnapshot snapshot)
    {
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

                CompareEntries(entry, otherEntry, diff);
            }
            else
            {
                // entry does not exist in older snapshot
                
                if (entry.HashHex != null && otherHashMap.TryGetValue(entry.HashHex, out otherEntry))
                {
                    // entry was moved

                    diff.AddMovedEntry(entry, otherEntry);
                    otherMovedEntries.Add(otherEntry);
                    CompareEntries(entry, otherEntry, diff, checkModified: false, changed: true);
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
        bool changed = false,
        bool checkModified = true)
    {
        if (entry.Type != other.Type)
        {
            // TODO
            throw new NotImplementedException();
        }

        if (checkModified
            && (
                !CheckEntryFileSizesMatch(entry, other).GetValueOrDefault(true)
                || entry.HashHex != other.HashHex))
        {
            diff.AddModifiedEntry(entry, other);
            changed = true;
        }

        if (!CheckEntryTimesMatch(entry, other))
        {
            diff.AddTouchedEntry(entry, other);
            changed = true;
        }

        if (!changed)
        {
            diff.AddUnchangedEntry(entry);
        }
    }

    private static bool CheckEntryTimesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.CreatedTime == other.CreatedTime && entry.LastModifiedTime == other.LastModifiedTime;
    }

    private static bool? CheckEntryFileSizesMatch(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        return entry.FileSize.HasValue && other.FileSize.HasValue
            ? entry.FileSize.Value == other.FileSize.Value
            : null;
    }
}
