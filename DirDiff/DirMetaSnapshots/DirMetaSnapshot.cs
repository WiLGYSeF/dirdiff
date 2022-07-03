namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshot
{
    public ICollection<DirMetaSnapshotEntry> Entries { get; } = new List<DirMetaSnapshotEntry>();

    public DirMetaSnapshot() { }

    internal void AddEntry(DirMetaSnapshotEntry entry)
    {
        Entries.Add(entry);
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

        var createdEntries = new List<DirMetaSnapshotEntry>();
        var deletedEntries = new List<DirMetaSnapshotEntry>();
        var modifiedEntries = new List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)>();
        var movedEntries = new List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)>();
        var touchedEntries = new List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)>();
        var unchangedEntries = new List<DirMetaSnapshotEntry>();

        foreach (var entry in entriesMap.Values)
        {
            if (otherEntriesMap.TryGetValue(entry.Path, out var otherEntry))
            {
                // entry exists in older snapshot

                CompareEntries(entry, otherEntry, modifiedEntries, touchedEntries, unchangedEntries);
            }
            else
            {
                // entry does not exist in older snapshot
                
                if (entry.HashHex != null && otherHashMap.TryGetValue(entry.HashHex, out otherEntry))
                {
                    // entry was moved

                    movedEntries.Add((otherEntry, entry));
                    CompareEntries(entry, otherEntry, null, touchedEntries, null);
                }
                else
                {
                    // entry was created

                    createdEntries.Add(entry);
                }
            }
        }

        foreach (var otherEntry in otherEntriesMap.Values)
        {
            if (!entriesMap.TryGetValue(otherEntry.Path, out var entry))
            {
                // exists in older snapshot but not newer
                deletedEntries.Add(otherEntry);
            }

            // other cases have already been handled
        }

        return diff;
    }

    private void CompareEntries(
        DirMetaSnapshotEntry entry,
        DirMetaSnapshotEntry other,
        List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)>? modifiedEntries,
        List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> touchedEntries,
        List<DirMetaSnapshotEntry>? unchangedEntries)
    {
        var changed = false;

        if (entry.Type != other.Type)
        {
            throw new NotImplementedException();
        }

        if (modifiedEntries != null
            && (
                !CheckEntryFileSizesMatch(entry, other).GetValueOrDefault(true)
                || entry.HashHex != other.HashHex))
        {
            modifiedEntries.Add((other, entry));
            changed = true;
        }

        if (!CheckEntryTimesMatch(entry, other))
        {
            touchedEntries.Add((other, entry));
            changed = true;
        }

        if (!changed && unchangedEntries != null)
        {
            unchangedEntries.Add(entry);
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
