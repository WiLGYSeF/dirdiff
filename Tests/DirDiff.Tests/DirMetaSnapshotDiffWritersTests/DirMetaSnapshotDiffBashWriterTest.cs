using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshots;
using DirDiff.Tests.Utils;
using System.Text;

namespace DirDiff.Tests.DirMetaSnapshotDiffWritersTests;

public class DirMetaSnapshotDiffBashWriterTest
{
    [Fact]
    public async Task Write_Diff()
    {
        var stream = new MemoryStream();

        var directorySeparator = '/';
        var firstPrefix = "test/";
        var secondPrefix = "abc/";

        var firstSnapshot = new DirMetaSnapshot(directorySeparator);
        var secondSnapshot = new DirMetaSnapshot(directorySeparator);

        var createdEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(secondPrefix))
            .Build();
        secondSnapshot.AddEntry(createdEntry);

        var firstModifiedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix))
            .Build();
        firstSnapshot.AddEntry(firstModifiedEntry);
        var secondModifiedEntry = new DirMetaSnapshotEntryBuilder().From(firstModifiedEntry)
            .WithPath(PathWithDifferentPrefix(firstModifiedEntry.Path, firstPrefix, secondPrefix))
            .WithRandomHash()
            .Build();
        secondSnapshot.AddEntry(secondModifiedEntry);

        var firstCopiedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix))
            .Build();
        firstSnapshot.AddEntry(firstCopiedEntry);
        var secondCopiedEntry = new DirMetaSnapshotEntryBuilder().From(firstCopiedEntry)
            .WithPath(PathWithDifferentPrefix(firstCopiedEntry.Path, firstPrefix, secondPrefix))
            .Build();
        secondSnapshot.AddEntry(secondCopiedEntry);
        var secondCopiedEntryCopy = new DirMetaSnapshotEntryBuilder().From(firstCopiedEntry)
            .WithPath(RandomPathWithPrefix(secondPrefix))
            .Build();
        secondSnapshot.AddEntry(secondCopiedEntryCopy);

        var firstMovedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix))
            .Build();
        firstSnapshot.AddEntry(firstMovedEntry);
        var secondMovedEntry = new DirMetaSnapshotEntryBuilder().From(firstMovedEntry)
            .WithPath(RandomPathWithPrefix(secondPrefix))
            .Build();
        secondSnapshot.AddEntry(secondMovedEntry);

        var firstTouchedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix))
            .Build();
        firstSnapshot.AddEntry(firstTouchedEntry);
        var secondTouchedEntry = new DirMetaSnapshotEntryBuilder().From(firstTouchedEntry)
            .WithPath(PathWithDifferentPrefix(firstTouchedEntry.Path, firstPrefix, secondPrefix))
            .WithRandomLastModifiedTime()
            .Build();
        secondSnapshot.AddEntry(secondTouchedEntry);

        var deletedEntry = new DirMetaSnapshotEntryBuilder()
            .WithPath(RandomPathWithPrefix(firstPrefix))
            .Build();
        firstSnapshot.AddEntry(deletedEntry);

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffBashWriter();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        ShouldBeCreateCommand(lines[0], createdEntry, firstSnapshot, secondSnapshot);
        ShouldBeModifyCommand(lines[1], firstModifiedEntry, secondModifiedEntry);
        ShouldBeCopyCommand(lines[2], firstCopiedEntry, secondCopiedEntryCopy, firstSnapshot, secondSnapshot);
        ShouldBeMoveCommand(lines[3], firstMovedEntry, secondMovedEntry, firstSnapshot, secondSnapshot);
        ShouldBeTouchCommand(lines[4], firstTouchedEntry, secondTouchedEntry);
        ShouldBeDeleteCommand(lines[5], deletedEntry);
    }

    private static void ShouldBeCreateCommand(string command, DirMetaSnapshotEntry entry, DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        command.ShouldBe($"cp -- '{entry.Path}' '{firstSnapshot.Prefix + secondSnapshot.PathWithoutPrefix(entry.Path)}'");
    }

    private static void ShouldBeModifyCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry)
    {
        command.ShouldBe($"cp -- '{secondEntry.Path}' '{firstEntry.Path}'");
    }

    private static void ShouldBeCopyCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry, DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        command.ShouldBe($"cp -- '{firstEntry.Path}' '{firstSnapshot.Prefix + secondSnapshot.PathWithoutPrefix(secondEntry.Path)}'");
    }

    private static void ShouldBeMoveCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry, DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        command.ShouldBe($"mv -- '{firstEntry.Path}' '{firstSnapshot.Prefix + secondSnapshot.PathWithoutPrefix(secondEntry.Path)}'");
    }

    private static void ShouldBeTouchCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry)
    {
        command.ShouldBe($"touch -d \"$(stat -c %y -- '{secondEntry.Path}')\" -- '{firstEntry.Path}'");
    }

    private static void ShouldBeDeleteCommand(string command, DirMetaSnapshotEntry entry)
    {
        command.ShouldBe($"rm -- '{entry.Path}'");
    }

    private static string RandomPathWithPrefix(string prefix)
    {
        return prefix + TestUtils.RandomPath(3);
    }

    private static string PathWithDifferentPrefix(string path, string firstPrefix, string secondPrefix)
    {
        return secondPrefix + path[firstPrefix.Length..];
    }
}
