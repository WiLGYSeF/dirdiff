namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotDiffEntryPair
{
    public DirMetaSnapshotEntry First { get; }

    public DirMetaSnapshotEntry Second { get; }

    internal DirMetaSnapshotDiffEntryPair(DirMetaSnapshotEntry first, DirMetaSnapshotEntry second)
    {
        First = first;
        Second = second;
    }
}
