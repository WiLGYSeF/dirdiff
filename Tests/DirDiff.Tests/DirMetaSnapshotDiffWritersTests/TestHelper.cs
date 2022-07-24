using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
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
        var firstFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = firstSnapshot.DirectorySeparator,
        };
        var secondFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = secondSnapshot.DirectorySeparator,
        };

        var createdEntry = secondFactory.Create()
            .WithRandomPath(secondPrefix)
            .Build();
        secondSnapshot.AddEntry(createdEntry);

        var firstModifiedEntry = firstFactory.Create()
            .WithRandomPath(firstPrefix)
            .Build();
        firstSnapshot.AddEntry(firstModifiedEntry);
        var secondModifiedEntry = secondFactory.Create(firstModifiedEntry)
            .WithPath(PathWithDifferentPrefix(
                firstSnapshot.ChangePathDirectorySeparator(firstModifiedEntry.Path, secondSnapshot.DirectorySeparator),
                firstPrefix,
                secondPrefix))
            .WithRandomHash()
            .Build();
        secondSnapshot.AddEntry(secondModifiedEntry);

        var firstCopiedUnchangedEntry = firstFactory.Create()
            .WithRandomPath(firstPrefix)
            .Build();
        firstSnapshot.AddEntry(firstCopiedUnchangedEntry);
        var secondUnchangedEntry = secondFactory.Create(firstCopiedUnchangedEntry)
            .WithPath(PathWithDifferentPrefix(
                firstSnapshot.ChangePathDirectorySeparator(firstCopiedUnchangedEntry.Path, secondSnapshot.DirectorySeparator),
                firstPrefix,
                secondPrefix))
            .Build();
        secondSnapshot.AddEntry(secondUnchangedEntry);
        var secondCopiedEntry = secondFactory.Create(firstCopiedUnchangedEntry)
            .WithRandomPath(secondPrefix)
            .Build();
        secondSnapshot.AddEntry(secondCopiedEntry);

        var firstMovedEntry = firstFactory.Create()
            .WithRandomPath(firstPrefix)
            .Build();
        firstSnapshot.AddEntry(firstMovedEntry);
        var secondMovedEntry = secondFactory.Create(firstMovedEntry)
            .WithRandomPath(secondPrefix)
            .Build();
        secondSnapshot.AddEntry(secondMovedEntry);

        var firstTouchedEntry = firstFactory.Create()
            .WithRandomPath(firstPrefix)
            .Build();
        firstSnapshot.AddEntry(firstTouchedEntry);
        var secondTouchedEntry = secondFactory.Create(firstTouchedEntry)
            .WithPath(PathWithDifferentPrefix(
                firstSnapshot.ChangePathDirectorySeparator(firstTouchedEntry.Path, secondSnapshot.DirectorySeparator),
                firstPrefix,
                secondPrefix))
            .WithRandomLastModifiedTime()
            .Build();
        secondSnapshot.AddEntry(secondTouchedEntry);

        var deletedEntry = firstFactory.Create()
            .WithRandomPath(firstPrefix)
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

    public static string GetEntryPath(
        DirMetaSnapshotEntry entry,
        DirMetaSnapshot firstSnapshot,
        DirMetaSnapshot secondSnapshot,
        char? directorySeparator = null,
        string? firstPrefix = null,
        string? secondPrefix = null)
    {
        var snapshot = firstSnapshot.ContainsPath(entry.Path) ? firstSnapshot : secondSnapshot;
        var path = entry.Path;

        if (snapshot == firstSnapshot)
        {
            if (firstPrefix != null)
            {
                path = firstPrefix + snapshot.PathWithoutPrefix(path);
            }
        }
        else
        {
            if (secondPrefix != null)
            {
                path = secondPrefix + snapshot.PathWithoutPrefix(path);
            }
        }

        if (directorySeparator.HasValue)
        {
            path = snapshot.ChangePathDirectorySeparator(path, directorySeparator.Value);
        }

        return path;
    }

    private static string PathWithDifferentPrefix(string path, string firstPrefix, string secondPrefix)
    {
        return secondPrefix + path[firstPrefix.Length..];
    }

    public class DiffSchema
    {
        public ICollection<DirMetaSnapshotEntrySchema>? Created { get; set; }
        public ICollection<DirMetaSnapshotEntrySchema>? Deleted { get; set; }
        public ICollection<DiffEntryPairSchema>? Modified { get; set; }
        public ICollection<DiffEntryPairSchema>? Copied { get; set; }
        public ICollection<DiffEntryPairSchema>? Moved { get; set; }
        public ICollection<DiffEntryPairSchema>? Touched { get; set; }
        public ICollection<DirMetaSnapshotEntrySchema>? Unchanged { get; set; }
    }

    public class DiffEntryPairSchema
    {
        public DirMetaSnapshotEntrySchema? First { get; set; }
        public DirMetaSnapshotEntrySchema? Second { get; set; }
    }

    public class BasicDiffResult
    {
        public DirMetaSnapshotEntry? CreatedEntry { get; init; }
        public DirMetaSnapshotEntry? DeletedEntry { get; init; }
        public DirMetaSnapshotEntry? FirstModifiedEntry { get; init; }
        public DirMetaSnapshotEntry? SecondModifiedEntry { get; init; }
        public DirMetaSnapshotEntry? FirstCopiedEntry { get; init; }
        public DirMetaSnapshotEntry? SecondCopiedEntry { get; init; }
        public DirMetaSnapshotEntry? FirstMovedEntry { get; init; }
        public DirMetaSnapshotEntry? SecondMovedEntry { get; init; }
        public DirMetaSnapshotEntry? FirstTouchedEntry { get; init; }
        public DirMetaSnapshotEntry? SecondTouchedEntry { get; init; }
        public DirMetaSnapshotEntry? FirstUnchangedEntry { get; init; }
        public DirMetaSnapshotEntry? SecondUnchangedEntry { get; init; }
    }
}
