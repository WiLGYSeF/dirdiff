using DirDiff.DirMetaSnapshots;
using static DirDiff.Tests.TestUtils;

namespace DirDiff.Tests.DirMetaSnapshotTests;

public class DirMetaSnapshotTest
{
    [Fact]
    public void Should_Compare_Create_Entries()
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

        diff.CreatedEntries.Count.ShouldBe(expectedCreatedEntries.Count);
        foreach (var entry in expectedCreatedEntries)
        {
            diff.CreatedEntries.ShouldContain(e => entry.Path == e.Path);
        }

        diff.DeletedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Compare_Delete_Entries()
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

        diff.DeletedEntries.Count.ShouldBe(expectedDeletedEntries.Count);
        foreach (var entry in expectedDeletedEntries)
        {
            diff.DeletedEntries.ShouldContain(e => entry.Path == e.Path);
        }

        diff.CreatedEntries.ShouldBeEmpty();
        diff.ModifiedEntries.ShouldBeEmpty();
        diff.MovedEntries.ShouldBeEmpty();
        diff.TouchedEntries.ShouldBeEmpty();
        diff.UnchangedEntries.ShouldBeEmpty();
    }
}
