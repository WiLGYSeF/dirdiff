using Wilgysef.DirDiff.DirMetaSnapshots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wilgysef.DirDiff.DirMetaSnapshotComparers;

public interface IDirMetaSnapshotComparer
{
    /// <summary>
    /// Snapshot comparer options.
    /// </summary>
    DirMetaSnapshotComparerOptions Options { get; }

    /// <summary>
    /// Configures the snapshot comparer.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    IDirMetaSnapshotComparer Configure(Action<DirMetaSnapshotComparerOptions> action);

    /// <summary>
    /// Compares two snapshots and creates a diff.
    /// </summary>
    /// <param name="firstSnapshot">Older snapshot.</param>
    /// <param name="secondSnapshot">Newer snapshot.</param>
    /// <returns>Snapshot diff.</returns>
    DirMetaSnapshotDiff Compare(DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot);
}
