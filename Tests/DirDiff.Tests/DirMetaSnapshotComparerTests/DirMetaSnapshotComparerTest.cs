using DirDiff.DirMetaSnapshotComparers;
using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirDiff.Tests.DirMetaSnapshotComparerTests;

public class DirMetaSnapshotComparerTest
{
    #region Basic Tests

    [Fact]
    public void Compare_Created_Entries()
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

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Deleted_Entries()
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

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry).Build();
            second.AddEntry(entryCopy);
            expectedUnchagedEntries.Add(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomPath()
                .Build();
            second.AddEntry(entryCopy);
            expectedCopiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomFileSize()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomPath()
                .Build();
            second.AddEntry(entryCopy);
            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries)
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            second.AddEntry(new DirMetaSnapshotEntryBuilder(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var builder = new DirMetaSnapshotEntryBuilder(entry)
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

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            second.AddEntry(new DirMetaSnapshotEntryBuilder(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomPath()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            second.AddEntry(new DirMetaSnapshotEntryBuilder(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomHash()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Created_Deleted_Modified_Moved_Entries()
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
            second.AddEntry(new DirMetaSnapshotEntryBuilder(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2).Take(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
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

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var builder = new DirMetaSnapshotEntryBuilder(entry);
            var entryCopy1 = builder.Build();
            var entryCopy2 = builder.WithRandomPath().Build();
            second.AddEntry(entryCopy1);
            second.AddEntry(entryCopy2);
            expectedCopiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy2));
            expectedUnchagedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomPath()
                .Build();
            second.AddEntry(entryCopy);

            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var builder = new DirMetaSnapshotEntryBuilder(entry);
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
                var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
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

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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

    [Fact]
    public void Compare_Moved_Entries_Hash_Already_Moved()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCopiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        var entry = new DirMetaSnapshotEntryBuilder().Build();
        first.AddEntry(entry);

        var entryCopy1 = new DirMetaSnapshotEntryBuilder(entry)
            .WithRandomPath()
            .Build();
        second.AddEntry(entryCopy1);
        expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy1));

        var entryCopy2 = new DirMetaSnapshotEntryBuilder(entry)
            .WithRandomPath()
            .Build();
        second.AddEntry(entryCopy2);
        expectedCopiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy2));

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = true;
        }).Compare(first, second);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEquivalentTo(expectedCopiedEntries);
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
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
            second.AddEntry(new DirMetaSnapshotEntryBuilder(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithNoHash()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = true;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entry);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = sizeAndTimeMatch;
            options.UnknownAssumeModified = true;
        }).Compare(first, second);

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
            second.AddEntry(new DirMetaSnapshotEntryBuilder(entry).Build());
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithNoHash()
                .WithLastModifiedTime(entry.LastModifiedTime + new TimeSpan(0, 0, 5))
                .Build();
            second.AddEntry(entryCopy);
            expectedUnchangedEntries.Add(entry);
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = true;
            options.TimeWindow = TimeSpan.FromMinutes(1);
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomPath()
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = true;
        }).Compare(first, second);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Moved_Entries_Size_Time_Already_Moved()
    {
        var first = new DirMetaSnapshot();
        var second = new DirMetaSnapshot();

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedMovedEntries = new List<DirMetaSnapshotDiffEntryPair>();

        var entry = new DirMetaSnapshotEntryBuilder().Build();
        first.AddEntry(entry);

        var entryCopy1 = new DirMetaSnapshotEntryBuilder(entry)
            .WithRandomPath()
            .WithNoHash()
            .Build();
        second.AddEntry(entryCopy1);
        expectedMovedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy1));

        var entryCopy2 = new DirMetaSnapshotEntryBuilder(entry)
            .WithRandomPath()
            .WithNoHash()
            .Build();
        second.AddEntry(entryCopy2);
        expectedCreatedEntries.Add(entryCopy2);

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = true;
        }).Compare(first, second);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomPath()
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedCreatedEntries.Add(entryCopy);
            expectedDeletedEntries.Add(entry);
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.SizeAndTimeMatch = true;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomFileSize()
                .WithRandomCreatedTime()
                .WithRandomLastModifiedTime()
                .WithRandomHash(HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithFileSize(null)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithFileSize(null)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entry);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomFileSize()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomHash(HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomFileSize()
                .WithRandomHash(HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithFileSize(null)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithFileSize(null)
                .WithNoHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithRandomCreatedTime()
                .WithRandomLastModifiedTime()
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
            var entryCopy = new DirMetaSnapshotEntryBuilder(entry)
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = new DirMetaSnapshotComparer().Configure(options =>
        {
            options.UnknownAssumeModified = unknownAssumeModified;
        }).Compare(first, second);

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
        var prefix = "abc/test/";

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var snapshot = new DirMetaSnapshot(directorySeparator);

        snapshot.AddEntry(factory.Create()
            .WithRandomPath(prefix)
            .Build());
        snapshot.Prefix.ShouldBe(
            snapshot.Entries.First().Path
                .Split(directorySeparator)[..^1]
                .Join(directorySeparator) + directorySeparator);

        snapshot.AddEntry(factory.Create()
            .WithRandomPath(prefix)
            .Build());
        snapshot.Prefix.ShouldBe(prefix);

        snapshot.AddEntry(factory.Create()
            .WithRandomPath(prefix)
            .Build());
        snapshot.Prefix.ShouldBe(prefix);
    }

    [Fact]
    public void Compare_Created_Deleted_Modified_Moved_Entries_Different_Prefixes()
    {
        var directorySeparator = '/';
        var firstPrefix = "abc/test/";
        var secondPrefix = "def/anothertest/";

        var firstFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };
        var secondFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var first = new DirMetaSnapshot(directorySeparator);
        var second = new DirMetaSnapshot(directorySeparator);

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();
        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 8; i++)
        {
            var entry = firstFactory.Create()
                .WithRandomPath(firstPrefix)
                .Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var entryCopy = secondFactory.Create(entry)
                .WithPath(secondPrefix + entry.Path[firstPrefix.Length..])
                .Build();
            second.AddEntry(entryCopy);
            expectedUnchangedEntries.Add(entryCopy);
        }

        foreach (var entry in first.Entries.Skip(2).Take(2))
        {
            var entryCopy = secondFactory.Create(entry)
                .WithPath(secondPrefix + entry.Path[firstPrefix.Length..])
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        foreach (var entry in first.Entries.Skip(4))
        {
            var otherEntry = firstFactory.Create()
                .WithRandomPath(secondPrefix)
                .Build();
            second.AddEntry(otherEntry);
            expectedCreatedEntries.Add(otherEntry);
            expectedDeletedEntries.Add(entry);
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Created_Deleted_Modified_Moved_Entries_Different_DirectorySeparator()
    {
        var firstDirectorySeparator = '/';
        var secondDirectorySeparator = '\\';

        var firstFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = firstDirectorySeparator,
        };
        var secondFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = secondDirectorySeparator,
        };

        var first = new DirMetaSnapshot(firstDirectorySeparator);
        var second = new DirMetaSnapshot(secondDirectorySeparator);

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();
        var expectedModifiedEntries = new List<DirMetaSnapshotDiffEntryPair>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 8; i++)
        {
            var entry = firstFactory.Create().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            var entryCopy = secondFactory.Create(entry)
                .WithPath(first.ChangePathDirectorySeparator(entry.Path, secondDirectorySeparator))
                .Build();
            second.AddEntry(entryCopy);
            expectedUnchangedEntries.Add(entryCopy);
        }

        foreach (var entry in first.Entries.Skip(2).Take(2))
        {
            var entryCopy = secondFactory.Create(entry)
                .WithPath(first.ChangePathDirectorySeparator(entry.Path, secondDirectorySeparator))
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        foreach (var entry in first.Entries.Skip(4))
        {
            var otherEntry = firstFactory.Create().Build();
            second.AddEntry(otherEntry);
            expectedCreatedEntries.Add(otherEntry);
            expectedDeletedEntries.Add(entry);
        }

        var diff = new DirMetaSnapshotComparer().Compare(first, second);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.CopiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }
}
