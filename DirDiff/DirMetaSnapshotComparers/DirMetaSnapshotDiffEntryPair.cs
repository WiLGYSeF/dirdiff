using DirDiff.DirMetaSnapshots;

namespace DirDiff.DirMetaSnapshotComparers;

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

    public DirMetaSnapshotDiffEntryPair(DirMetaSnapshotEntry first, DirMetaSnapshotEntry second)
    {
        First = first;
        Second = second;
    }
}
