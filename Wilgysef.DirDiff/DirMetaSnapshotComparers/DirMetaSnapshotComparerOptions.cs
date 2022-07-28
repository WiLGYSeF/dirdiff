namespace Wilgysef.DirDiff.DirMetaSnapshotComparers;

public class DirMetaSnapshotComparerOptions
{
    /// <summary>
    /// Indicates if matching entry last modified times and file sizes can be considered a match if there are no entry hashes.
    /// Otherwise, they are considered unknown.
    /// </summary>
    public bool SizeAndTimeMatch { get; set; } = true;

    /// <summary>
    /// Indicates if unknown entry comparisons should be treated as modifications.
    /// </summary>
    public bool UnknownAssumeModified { get; set; } = true;

    /// <summary>
    /// Maximum difference in times before entries are considered different.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.Zero;
}
