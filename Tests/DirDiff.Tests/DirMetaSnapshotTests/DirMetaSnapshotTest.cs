using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Tests.Utils;

namespace DirDiff.Tests.DirMetaSnapshotTests;

public class DirMetaSnapshotTest
{
    #region Basic Tests

    [Fact]
    public void Compare_Create_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            second.AddEntry(entry);
            expectedCreatedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Delete_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
            expectedDeletedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Modified_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Copied_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCopiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchagedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .Build();
            second.AddEntry(entryCopy);
            expectedUnchagedEntries.Add(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .Build();
            second.AddEntry(entryCopy);
            expectedCopiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEquivalentTo(expectedCopiedEntries);
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchagedEntries);
    }

    [Fact]
    public void Compare_Modified_Entries_Changed_FileSize()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomFileSize()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Moved_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .Build();
            second.AddEntry(entryCopy);
            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Touched_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedTouchedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Unchanged_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
            second.AddEntry(new DirMetaSnapshotEntryBuilder().From(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Type_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 2; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithFileType(FileType.File)
                .Build();
            first.AddEntry(entry);
        }

        for (var i = 0; i < 2; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithFileType(FileType.Directory)
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var oppositeType = entry.Type switch
            {
                FileType.Directory => FileType.File,
                _ => FileType.Directory,
            };
            var builder = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithFileType(oppositeType);

            if (oppositeType == FileType.Directory)
            {
                builder = builder.WithNoHash();
            }

            var entryCopy = builder.Build();

            second.AddEntry(entryCopy);
            expectedCreatedEntries.Add(entryCopy);
            expectedDeletedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    #endregion

    [Fact]
    public void Compare_Moved_Touched_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedTouchedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            second.AddEntry(new DirMetaSnapshotEntryBuilder().From(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Modified_Touched_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedTouchedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            second.AddEntry(new DirMetaSnapshotEntryBuilder().From(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomHash()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Create_Delete_Modified_Moved_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();
        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 8; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            second.AddEntry(new DirMetaSnapshotEntryBuilder().From(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2).Take(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        foreach (var entry in first.Entries.Skip(4))
        {
            var otherEntry = new DirMetaSnapshotEntryBuilder().Build();
            second.AddEntry(otherEntry);
            expectedCreatedEntries.Add(otherEntry);
            expectedDeletedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Copied_Moved_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCopiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchagedEntries = new List<DirMetaSnapshotEntry>();

        var hashAlgorithm = HashAlgorithm.SHA256;
        var hash = TestUtils.RandomHash(hashAlgorithm);
        var fileSize = TestUtils.RandomLong(1024 * 1024);

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithHash(hashAlgorithm, hash)
                .WithFileSize(fileSize)
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var builder = new DirMetaSnapshotEntryBuilder().From(entry);
            var entryCopy1 = builder.Build();
            var entryCopy2 = builder.WithRandomPath().Build();
            second.AddEntry(entryCopy1);
            second.AddEntry(entryCopy2);
            expectedCopiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy2));
            expectedUnchagedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .Build();
            second.AddEntry(entryCopy);

            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchagedEntries);

        diff.CopiedEntries.Count.ShouldBe(expectedCopiedEntries.Count);
        foreach (var copiedEntry in expectedCopiedEntries)
        {
            diff.CopiedEntries.ShouldContain(p => p.First.Hash!.SequenceEqual(copiedEntry.First.Hash!));
            diff.CopiedEntries.ShouldContain(p => p.Second == copiedEntry.Second);
        }

        diff.MovedEntries.Count.ShouldBe(expectedMovedEntries.Count);
        foreach (var movedEntry in expectedMovedEntries)
        {
            diff.MovedEntries.ShouldContain(p => p.First == movedEntry.First);
            diff.MovedEntries.ShouldContain(p => p.Second == movedEntry.Second);
        }
    }

    [Fact]
    public void Compare_Copied_Moved_Deleted_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();
        var expectedCopiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchagedEntries = new List<DirMetaSnapshotEntry>();

        var hashAlgorithm = HashAlgorithm.SHA256;
        var hash = TestUtils.RandomHash(hashAlgorithm);
        var fileSize = TestUtils.RandomLong(1024 * 1024);

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithHash(hashAlgorithm, hash)
                .WithFileSize(fileSize)
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var builder = new DirMetaSnapshotEntryBuilder().From(entry);
            var entryCopy1 = builder.Build();
            var entryCopy2 = builder.WithRandomPath().Build();
            second.AddEntry(entryCopy1);
            second.AddEntry(entryCopy2);
            expectedCopiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy2));
            expectedUnchagedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            if (expectedMovedEntries.Count == 0)
            {
                var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                    .WithRandomPath()
                    .Build();
                second.AddEntry(entryCopy);

                expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            }
            else
            {
                expectedDeletedEntries.Add(entry);
            }
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchagedEntries);

        diff.CopiedEntries.Count.ShouldBe(expectedCopiedEntries.Count);
        foreach (var copiedEntry in expectedCopiedEntries)
        {
            diff.CopiedEntries.ShouldContain(p => p.First.Hash!.SequenceEqual(copiedEntry.First.Hash!));
            diff.CopiedEntries.ShouldContain(p => p.Second == copiedEntry.Second);
        }

        diff.MovedEntries.Count.ShouldBe(expectedMovedEntries.Count);
        foreach (var movedEntry in expectedMovedEntries)
        {
            diff.MovedEntries.ShouldContain(p => p.First == movedEntry.First);
            diff.MovedEntries.ShouldContain(p => p.Second == movedEntry.Second);
        }
    }

    #region Size and Time Checks

    [Fact]
    public void Compare_Size_Time_Entries()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedTouchedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            second.AddEntry(new DirMetaSnapshotEntryBuilder().From(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithNoHash()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, sizeAndTimeMatch: true);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Compare_Size_Time_Match_Entries(bool sizeAndTimeMatch)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entry);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(
            first,
            sizeAndTimeMatch: sizeAndTimeMatch,
            unknownAssumeModified: true);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();

        if (sizeAndTimeMatch)
        {
            diff.ModifiedEntries.ShouldBeEmpty();
            diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
        }
        else
        {
            diff.ModifiedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.UnchangedEntries.ShouldBeEmpty();
        }
    }

    [Fact]
    public void Compare_Time_Entries_Window()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            second.AddEntry(new DirMetaSnapshotEntryBuilder().From(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithNoHash()
                .WithLastModifiedTime(entry.LastModifiedTime + new TimeSpan(0, 0, 5))
                .Build();
            second.AddEntry(entryCopy);
            expectedUnchangedEntries.Add(entry);
        }

        var diff = second.Compare(
            first,
            sizeAndTimeMatch: true,
            window: new TimeSpan(0, 1, 0));

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.Count.ShouldBe(expectedUnchangedEntries.Count);
    }

    [Fact]
    public void Compare_Moved_Entries_Size_Time()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, sizeAndTimeMatch: true);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Moved_Entries_Size_Time_Different_Hash()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedCreatedEntries.Add(entryCopy);
            expectedDeletedEntries.Add(entry);
        }

        var diff = second.Compare(first, sizeAndTimeMatch: true);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    #endregion

    #region Null Checks

    [Fact]
    public void Compare_Old_Null()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedTouchedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithFileSize(null)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomFileSize()
                .WithRandomCreatedTime()
                .WithRandomLastModifiedTime()
                .WithRandomHash(HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_New_Null()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedTouchedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithFileSize(null)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_New_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithFileSize(null)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithFileSize(null)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entry);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();

        if (unknownAssumeModified)
        {
            diff.ModifiedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.TouchedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.UnchangedEntries.ShouldBeEmpty();
        }
        else
        {
            diff.ModifiedEntries.ShouldBeEmpty();
            diff.TouchedEntries.ShouldBeEmpty();
            diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_FileSize_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithFileSize(null)
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomFileSize()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomHash(HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_FileSize_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithFileSize(null)
                .WithNoHash()
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomFileSize()
                .WithRandomHash(HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();

        if (unknownAssumeModified)
        {
            diff.ModifiedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.UnchangedEntries.ShouldBeEmpty();
        }
        else
        {
            diff.ModifiedEntries.ShouldBeEmpty();
            diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_New_FileSize_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithFileSize(null)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_New_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_New_FileSize_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithFileSize(null)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();

        if (unknownAssumeModified)
        {
            diff.ModifiedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.UnchangedEntries.ShouldBeEmpty();
        }
        else
        {
            diff.ModifiedEntries.ShouldBeEmpty();
            diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_Time_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomCreatedTime()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();

        if (unknownAssumeModified)
        {
            diff.TouchedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.UnchangedEntries.ShouldBeEmpty();
        }
        else
        {
            diff.TouchedEntries.ShouldBeEmpty();
            diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_New_Time_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedEntries = new List<DirMetaSnapshotEntry>();
        var expectedEntryPairs = new List<DirMetaSnapshotDiffEntryPair>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();

        if (unknownAssumeModified)
        {
            diff.TouchedEntries.ShouldBeEquivalentTo(expectedEntryPairs);
            diff.UnchangedEntries.ShouldBeEmpty();
        }
        else
        {
            diff.TouchedEntries.ShouldBeEmpty();
            diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
        }
    }

    #endregion

    [Fact]
    public void Sets_Prefix()
    {
        var directorySeparator = '/';
        var snapshot = new DirMetaSnapshot(directorySeparator);

        var prefix = "abc/test/";

        snapshot.AddEntry(new DirMetaSnapshotEntryBuilder()
            .WithPath(prefix + TestUtils.RandomPath(3))
            .Build());
        snapshot.Prefix.ShouldBe(
            snapshot.Entries.First().Path
                .Split(directorySeparator)[..^1]
                .Join(directorySeparator) + directorySeparator);

        snapshot.AddEntry(new DirMetaSnapshotEntryBuilder()
            .WithPath(prefix + TestUtils.RandomPath(3))
            .Build());
        snapshot.Prefix.ShouldBe(prefix);

        snapshot.AddEntry(new DirMetaSnapshotEntryBuilder()
            .WithPath(prefix + TestUtils.RandomPath(3))
            .Build());
        snapshot.Prefix.ShouldBe(prefix);
    }

    [Fact]
    public void Compare_Create_Delete_Modified_Moved_Entries_Different_Prefixes()
    {
        var directorySeparator = '/';
        var first = new DirMetaSnapshot(directorySeparator);
        var second = new DirMetaSnapshot(directorySeparator);

        var firstPrefix = "abc/test/";
        var secondPrefix = "def/anothertest/";

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();
        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 8; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithPath(firstPrefix + TestUtils.RandomPath(3))
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithPath(secondPrefix + entry.Path[firstPrefix.Length..])
                .Build();
            second.AddEntry(entryCopy);
            expectedUnchangedEntries.Add(entryCopy);
        }

        foreach (var entry in first.Entries.Skip(2).Take(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithPath(secondPrefix + entry.Path[firstPrefix.Length..])
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        foreach (var entry in first.Entries.Skip(4))
        {
            var otherEntry = new DirMetaSnapshotEntryBuilder()
                .WithPath(secondPrefix + TestUtils.RandomPath(3))
                .Build();
            second.AddEntry(otherEntry);
            expectedCreatedEntries.Add(otherEntry);
            expectedDeletedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }
}
