using DirDiff.DirMetaSnapshots;

namespace DirDiff.Tests.Utils;

internal class DirMetaSnapshotEntryBuilderFactory
{
    public char DirectorySeparator { get; set; }

    public long FileSizeMin { get; set; } = 0;

    public long FileSizeMax { get; set; } = 1024 * 1024;

    public DirMetaSnapshotEntryBuilderFactory()
    {
        DirectorySeparator = Path.DirectorySeparatorChar;
    }

    public DirMetaSnapshotEntryBuilder Create()
    {
        return new DirMetaSnapshotEntryBuilder()
            .WithDirectorySeparator(DirectorySeparator)
            .WithFileSizeRange(FileSizeMin, FileSizeMax);
    }

    public DirMetaSnapshotEntryBuilder Create(DirMetaSnapshotEntry entry)
    {
        return new DirMetaSnapshotEntryBuilder(entry)
            .WithDirectorySeparator(DirectorySeparator)
            .WithFileSizeRange(FileSizeMin, FileSizeMax);
    }
}
