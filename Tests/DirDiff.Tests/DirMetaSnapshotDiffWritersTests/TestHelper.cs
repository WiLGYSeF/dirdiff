using DirDiff.DirMetaSnapshots;
using DirDiff.Tests.Utils;

namespace DirDiff.Tests.DirMetaSnapshotDiffWritersTests;

internal static class TestHelper
{
    public static BasicDiffResult SetUpBasicDiff(
        DirMetaSnapshot firstSnapshot,
        DirMetaSnapshot secondSnapshot,
        string firstPrefix,
        string secondPrefix)
    {
        var createdEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(secondPrefix, firstSnapshot.DirectorySeparator))
            .Build();
        secondSnapshot.AddEntry(createdEntry);

        var firstModifiedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix, firstSnapshot.DirectorySeparator))
            .Build();
        firstSnapshot.AddEntry(firstModifiedEntry);
        var secondModifiedEntry = new DirMetaSnapshotEntryBuilder().From(firstModifiedEntry)
            .WithPath(PathWithDifferentPrefix(firstModifiedEntry.Path, firstPrefix, secondPrefix))
            .WithRandomHash()
            .Build();
        secondSnapshot.AddEntry(secondModifiedEntry);

        var firstCopiedUnchangedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix, firstSnapshot.DirectorySeparator))
            .Build();
        firstSnapshot.AddEntry(firstCopiedUnchangedEntry);
        var secondUnchangedEntry = new DirMetaSnapshotEntryBuilder().From(firstCopiedUnchangedEntry)
            .WithPath(PathWithDifferentPrefix(firstCopiedUnchangedEntry.Path, firstPrefix, secondPrefix))
            .Build();
        secondSnapshot.AddEntry(secondUnchangedEntry);
        var secondCopiedEntry = new DirMetaSnapshotEntryBuilder().From(firstCopiedUnchangedEntry)
            .WithPath(RandomPathWithPrefix(secondPrefix, secondSnapshot.DirectorySeparator))
            .Build();
        secondSnapshot.AddEntry(secondCopiedEntry);

        var firstMovedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix, firstSnapshot.DirectorySeparator))
            .Build();
        firstSnapshot.AddEntry(firstMovedEntry);
        var secondMovedEntry = new DirMetaSnapshotEntryBuilder().From(firstMovedEntry)
            .WithPath(RandomPathWithPrefix(secondPrefix, secondSnapshot.DirectorySeparator))
            .Build();
        secondSnapshot.AddEntry(secondMovedEntry);

        var firstTouchedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix, firstSnapshot.DirectorySeparator))
            .Build();
        firstSnapshot.AddEntry(firstTouchedEntry);
        var secondTouchedEntry = new DirMetaSnapshotEntryBuilder().From(firstTouchedEntry)
            .WithPath(PathWithDifferentPrefix(firstTouchedEntry.Path, firstPrefix, secondPrefix))
            .WithRandomLastModifiedTime()
            .Build();
        secondSnapshot.AddEntry(secondTouchedEntry);

        var deletedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix, firstSnapshot.DirectorySeparator))
            .Build();
        firstSnapshot.AddEntry(deletedEntry);

        return new BasicDiffResult
        {
            CreatedEntry = createdEntry,
            DeletedEntry = deletedEntry,
            FirstModifiedEntry = firstModifiedEntry,
            SecondModifiedEntry = secondModifiedEntry,
            FirstCopiedEntry = firstCopiedUnchangedEntry,
            SecondCopiedEntry = secondCopiedEntry,
            FirstMovedEntry = firstMovedEntry,
            SecondMovedEntry = secondMovedEntry,
            FirstTouchedEntry = firstTouchedEntry,
            SecondTouchedEntry = secondTouchedEntry,
            FirstUnchangedEntry = firstCopiedUnchangedEntry,
            SecondUnchangedEntry = secondUnchangedEntry,
        };
    }

    private static string RandomPathWithPrefix(string prefix, char separator)
    {
        return prefix + TestUtils.RandomPath(3, separator);
    }

    private static string PathWithDifferentPrefix(string path, string firstPrefix, string secondPrefix)
    {
        return secondPrefix + path[firstPrefix.Length..];
    }

    public class BasicDiffResult
    {
        public DirMetaSnapshotEntry CreatedEntry { get; init; }
        public DirMetaSnapshotEntry DeletedEntry { get; init; }
        public DirMetaSnapshotEntry FirstModifiedEntry { get; init; }
        public DirMetaSnapshotEntry SecondModifiedEntry { get; init; }
        public DirMetaSnapshotEntry FirstCopiedEntry { get; init; }
        public DirMetaSnapshotEntry SecondCopiedEntry { get; init; }
        public DirMetaSnapshotEntry FirstMovedEntry { get; init; }
        public DirMetaSnapshotEntry SecondMovedEntry { get; init; }
        public DirMetaSnapshotEntry FirstTouchedEntry { get; init; }
        public DirMetaSnapshotEntry SecondTouchedEntry { get; init; }
        public DirMetaSnapshotEntry FirstUnchangedEntry { get; init; }
        public DirMetaSnapshotEntry SecondUnchangedEntry { get; init; }
    }
}
