﻿using DirDiff.DirMetaSnapshots;
using DirDiff.Tests.Utils;

namespace DirDiff.Tests.DirMetaSnapshotTests;

public class DirMetaSnapshotTest
{
    private string SnapshotPrefix = "";

    #region Basic Tests

    [Fact]
    public void Compare_Create_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Delete_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Modified_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Modified_Entries_Changed_FileSize()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Moved_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Touched_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_Unchanged_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
            second.AddEntry(entry);
            expectedUnchangedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    #endregion

    [Fact]
    public void Compare_Moved_Touched_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
            second.AddEntry(entry);
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
        diff.MovedEntries.ShouldBeEquivalentTo(expectedMovedEntries);
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Modified_Touched_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
            second.AddEntry(entry);
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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Create_Delete_Modified_And_Moved_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

        var expectedCreatedEntries = new List<DirMetaSnapshotEntry>();
        var expectedDeletedEntries = new List<DirMetaSnapshotEntry>();
        var expectedUnchangedEntries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            first.AddEntry(entry);
        }

        foreach (var entry in first.Entries.Take(2))
        {
            second.AddEntry(entry);
            expectedUnchangedEntries.Add(entry);
        }

        foreach (var entry in first.Entries.Skip(2))
        {
            var entryCopy = new DirMetaSnapshotEntryBuilder().From(entry)
                .WithRandomPath()
                .WithRandomHash()
                .Build();
            second.AddEntry(entryCopy);
            expectedCreatedEntries.Add(entryCopy);
            expectedDeletedEntries.Add(entry);
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEquivalentTo(expectedCreatedEntries);
        diff.DeletedEntries.ShouldBeEquivalentTo(expectedDeletedEntries);
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    [Fact]
    public void Compare_Size_Time_Entries()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
            second.AddEntry(entry);
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

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedUnchangedEntries);
    }

    #region Null Checks

    [Fact]
    public void Compare_Old_Null()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
                .WithRandomHash(Enums.HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedModifiedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
            expectedTouchedEntries.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEquivalentTo(expectedModifiedEntries);
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_New_Null()
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEquivalentTo(expectedTouchedEntries);
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_New_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
                .WithRandomHash(Enums.HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_Old_FileSize_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
                .WithRandomHash(Enums.HashAlgorithm.SHA256)
                .Build();
            second.AddEntry(entryCopy);
            expectedEntries.Add(entryCopy);
            expectedEntryPairs.Add(new DirMetaSnapshotDiffEntryPair(entry, entryCopy));
        }

        var diff = second.Compare(first, unknownAssumeModified: unknownAssumeModified);

        diff.CreatedEntries.ShouldBeEmpty();
        diff.DeletedEntries.ShouldBeEmpty();
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
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_New_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEquivalentTo(expectedEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compare_New_FileSize_Hash_Null(bool unknownAssumeModified)
    {
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
        var first = new DirMetaSnapshot(SnapshotPrefix);
        var second = new DirMetaSnapshot(SnapshotPrefix);

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
}
