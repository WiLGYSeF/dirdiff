namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotDiffEntryPair
{
    /// <summary>
    /// First entry.
    /// </summary>
    public DirMetaSnapshotEntry First { get; }

    /// <summary>
    /// Second entry.
    /// </summary>
    public DirMetaSnapshotEntry Second { get; }

    internal DirMetaSnapshotDiffEntryPair(DirMetaSnapshotEntry first, DirMetaSnapshotEntry second)
    {
        First = first;
        Second = second;
    }
}
