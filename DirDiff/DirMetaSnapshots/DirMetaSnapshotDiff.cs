namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotDiff
{
    public IReadOnlyCollection<DirMetaSnapshotEntry> CreatedEntries => _createdEntries;

    public IReadOnlyCollection<DirMetaSnapshotEntry> DeletedEntries => _deletedEntries;

    public IReadOnlyCollection<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> ModifiedEntries => _modifiedEntries;

    public IReadOnlyCollection<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> MovedEntries => _movedEntries;

    public IReadOnlyCollection<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> TouchedEntries => _touchedEntries;

    public IReadOnlyCollection<DirMetaSnapshotEntry> UnchangedEntries => _unchangedEntries;

    private readonly List<DirMetaSnapshotEntry> _createdEntries = new();
    private readonly List<DirMetaSnapshotEntry> _deletedEntries = new();
    private readonly List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> _modifiedEntries = new();
    private readonly List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> _movedEntries = new();
    private readonly List<(DirMetaSnapshotEntry OldEntry, DirMetaSnapshotEntry NewEntry)> _touchedEntries = new();
    private readonly List<DirMetaSnapshotEntry> _unchangedEntries = new();

    internal void AddCreatedEntry(DirMetaSnapshotEntry entry)
    {
        _createdEntries.Add(entry);
    }

    internal void AddDeletedEntry(DirMetaSnapshotEntry entry)
    {
        _deletedEntries.Add(entry);
    }

    internal void AddModifiedEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        _modifiedEntries.Add((other, entry));
    }

    internal void AddMovedEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        _movedEntries.Add((other, entry));
    }

    internal void AddTouchedEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        _touchedEntries.Add((other, entry));
    }

    internal void AddUnchangedEntry(DirMetaSnapshotEntry entry)
    {
        _unchangedEntries.Add(entry);
    }
}
