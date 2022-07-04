﻿namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotDiff
{
    public IReadOnlyCollection<DirMetaSnapshotEntry> CreatedEntries => _createdEntries;

    public IReadOnlyCollection<DirMetaSnapshotEntry> DeletedEntries => _deletedEntries;

    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> ModifiedEntries => _modifiedEntries;

    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> MovedEntries => _movedEntries;

    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> TouchedEntries => _touchedEntries;

    public IReadOnlyCollection<DirMetaSnapshotEntry> UnchangedEntries => _unchangedEntries;

    private readonly List<DirMetaSnapshotEntry> _createdEntries = new();
    private readonly List<DirMetaSnapshotEntry> _deletedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _modifiedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _movedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _touchedEntries = new();
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
        _modifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(other, entry));
    }

    internal void AddMovedEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        _movedEntries.Add(new DirMetaSnapshotDiffEntryPair(other, entry));
    }

    internal void AddTouchedEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        _touchedEntries.Add(new DirMetaSnapshotDiffEntryPair(other, entry));
    }

    internal void AddUnchangedEntry(DirMetaSnapshotEntry entry)
    {
        _unchangedEntries.Add(entry);
    }
}
