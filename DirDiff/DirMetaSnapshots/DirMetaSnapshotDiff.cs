namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotDiff
{
    internal DirMetaSnapshot FirstSnapshot => _firstSnapshot;

    internal DirMetaSnapshot SecondSnapshot => _secondSnapshot;

    /// <summary>
    /// Created entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotEntry> CreatedEntries => _createdEntries;

    /// <summary>
    /// Deleted entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotEntry> DeletedEntries => _deletedEntries;

    /// <summary>
    /// Modified entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> ModifiedEntries => _modifiedEntries;

    /// <summary>
    /// Copied entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> CopiedEntries => _copiedEntries;

    /// <summary>
    /// Moved entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> MovedEntries => _movedEntries;

    /// <summary>
    /// Touched entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> TouchedEntries => _touchedEntries;

    /// <summary>
    /// Unchanged entries.
    /// </summary>
    public IReadOnlyCollection<DirMetaSnapshotEntry> UnchangedEntries => _unchangedEntries;

    private readonly List<DirMetaSnapshotEntry> _createdEntries = new();
    private readonly List<DirMetaSnapshotEntry> _deletedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _modifiedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _copiedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _movedEntries = new();
    private readonly List<DirMetaSnapshotDiffEntryPair> _touchedEntries = new();
    private readonly List<DirMetaSnapshotEntry> _unchangedEntries = new();

    private readonly DirMetaSnapshot _firstSnapshot;
    private readonly DirMetaSnapshot _secondSnapshot;

    internal DirMetaSnapshotDiff(DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        _firstSnapshot = firstSnapshot;
        _secondSnapshot = secondSnapshot;
    }

    /// <summary>
    /// Gets the entry path without its prefix.
    /// </summary>
    /// <param name="entry">Entry.</param>
    /// <returns>Entry path without prefix.</returns>
    /// <exception cref="ArgumentException">Entry does not belong to snapshot diff.</exception>
    public string GetEntryPathWithoutPrefix(DirMetaSnapshotEntry entry)
    {
        if (_firstSnapshot.Entries.Contains(entry))
        {
            return _firstSnapshot.PathWithoutPrefix(entry.Path);
        }
        if (_secondSnapshot.Entries.Contains(entry))
        {
            return _secondSnapshot.PathWithoutPrefix(entry.Path);
        }

        throw new ArgumentException("Entry does not belong to snapshot diff.", nameof(entry));
    }

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

    internal void AddCopiedEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry other)
    {
        _copiedEntries.Add(new DirMetaSnapshotDiffEntryPair(other, entry));
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
