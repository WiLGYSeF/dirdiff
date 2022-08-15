using Wilgysef.DirDiff.DirMetaSnapshotComparers;
using Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.Tests.Utils;
using System.Text;

namespace Wilgysef.DirDiff.Tests.DirMetaSnapshotDiffWritersTests;

public class DirMetaSnapshotDiffBashWriterTest
{
    [Fact]
    public async Task Write_Diff()
    {
        var directorySeparator = '/';
        var firstPrefix = "test/";
        var secondPrefix = "abc/";

        var firstSnapshot = new DirMetaSnapshot(directorySeparator);
        var secondSnapshot = new DirMetaSnapshot(directorySeparator);

        var entries = TestHelper.SetUpBasicDiff(firstSnapshot, secondSnapshot, firstPrefix, secondPrefix);

        var diff = new DirMetaSnapshotComparer().Compare(firstSnapshot, secondSnapshot);

        var diffWriter = new DirMetaSnapshotDiffBashWriter();
        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        string First(DirMetaSnapshotEntry entry)
        {
            return firstPrefix + diff.GetEntryPathWithoutPrefix(entry);
        }

        string Second(DirMetaSnapshotEntry entry)
        {
            return diff.GetEntrySnapshot(entry)
                .ChangePathDirectorySeparator(entry.Path, secondSnapshot.DirectorySeparator);
        }

        lines.Length.ShouldBe(7);
        ShouldBeCreateCommand(lines[0], Second(entries.CreatedEntry!), First(entries.CreatedEntry!));
        ShouldBeModifyCommand(lines[1], Second(entries.SecondModifiedEntry!), First(entries.FirstModifiedEntry!));
        ShouldBeCopyCommand(lines[2], First(entries.FirstCopiedEntry!), First(entries.SecondCopiedEntry!));
        ShouldBeMoveCommand(lines[3], First(entries.FirstMovedEntry!), First(entries.SecondMovedEntry!));
        ShouldBeTouchCommand(lines[4], Second(entries.SecondTouchedEntry!), First(entries.FirstTouchedEntry!));
        ShouldBeDeleteCommand(lines[5], First(entries.DeletedEntry!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Write_Different_DirectorySeparator(bool forceFirstSeparator)
    {
        var firstDirectorySeparator = '/';
        var secondDirectorySeparator = '\\';
        var firstPrefix = "test/";
        var secondPrefix = "abc\\";

        var firstSnapshot = new DirMetaSnapshot(firstDirectorySeparator);
        var secondSnapshot = new DirMetaSnapshot(secondDirectorySeparator);

        var entries = TestHelper.SetUpBasicDiff(firstSnapshot, secondSnapshot, firstPrefix, secondPrefix);

        var diff = new DirMetaSnapshotComparer().Compare(firstSnapshot, secondSnapshot);

        var diffWriter = new DirMetaSnapshotDiffBashWriter().Configure(options =>
        {
            options.DirectorySeparator = forceFirstSeparator ? firstDirectorySeparator : null;
        });

        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        string First(DirMetaSnapshotEntry entry)
        {
            var snapshot = diff.GetEntrySnapshot(entry);
            return firstSnapshot.ChangePathDirectorySeparator(firstPrefix, forceFirstSeparator ? firstDirectorySeparator : secondSnapshot.DirectorySeparator)
                + snapshot.ChangePathDirectorySeparator(
                    snapshot.PathWithoutPrefix(entry.Path),
                    forceFirstSeparator ? firstDirectorySeparator : secondSnapshot.DirectorySeparator);
        }

        string Second(DirMetaSnapshotEntry entry)
        {
            return diff.GetEntrySnapshot(entry)
                .ChangePathDirectorySeparator(entry.Path, forceFirstSeparator ? firstDirectorySeparator : secondSnapshot.DirectorySeparator);
        }

        lines.Length.ShouldBe(7);
        ShouldBeCreateCommand(lines[0], Second(entries.CreatedEntry!), First(entries.CreatedEntry!));
        ShouldBeModifyCommand(lines[1], Second(entries.SecondModifiedEntry!), First(entries.FirstModifiedEntry!));
        ShouldBeCopyCommand(lines[2], First(entries.FirstCopiedEntry!), First(entries.SecondCopiedEntry!));
        ShouldBeMoveCommand(lines[3], First(entries.FirstMovedEntry!), First(entries.SecondMovedEntry!));
        ShouldBeTouchCommand(lines[4], Second(entries.SecondTouchedEntry!), First(entries.FirstTouchedEntry!));
        ShouldBeDeleteCommand(lines[5], First(entries.DeletedEntry!));
    }

    [Fact]
    public async Task Write_Different_Prefix()
    {
        var firstDirectorySeparator = '/';
        var secondDirectorySeparator = '\\';
        var firstPrefix = "test/";
        var secondPrefix = "abc\\";
        var firstNewPrefix = "newtest/";
        var secondNewPrefix = "newabc\\";

        var firstSnapshot = new DirMetaSnapshot(firstDirectorySeparator);
        var secondSnapshot = new DirMetaSnapshot(secondDirectorySeparator);

        var entries = TestHelper.SetUpBasicDiff(firstSnapshot, secondSnapshot, firstPrefix, secondPrefix);

        var diff = new DirMetaSnapshotComparer().Compare(firstSnapshot, secondSnapshot);

        var diffWriter = new DirMetaSnapshotDiffBashWriter().Configure(options =>
        {
            options.FirstPrefix = firstNewPrefix;
            options.SecondPrefix = secondNewPrefix;
        });

        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        string First(DirMetaSnapshotEntry entry)
        {
            var snapshot = diff.GetEntrySnapshot(entry);
            return firstSnapshot.ChangePathDirectorySeparator(firstNewPrefix, secondSnapshot.DirectorySeparator)
                + snapshot.ChangePathDirectorySeparator(
                    snapshot.PathWithoutPrefix(entry.Path),
                    secondSnapshot!.DirectorySeparator);
        }

        string Second(DirMetaSnapshotEntry entry)
        {
            var snapshot = diff.GetEntrySnapshot(entry);
            return secondNewPrefix
                + snapshot.ChangePathDirectorySeparator(
                    snapshot.PathWithoutPrefix(entry.Path),
                    secondSnapshot!.DirectorySeparator);
        }

        lines.Length.ShouldBe(7);
        ShouldBeCreateCommand(lines[0], Second(entries.CreatedEntry!), First(entries.CreatedEntry!));
        ShouldBeModifyCommand(lines[1], Second(entries.SecondModifiedEntry!), First(entries.FirstModifiedEntry!));
        ShouldBeCopyCommand(lines[2], First(entries.FirstCopiedEntry!), First(entries.SecondCopiedEntry!));
        ShouldBeMoveCommand(lines[3], First(entries.FirstMovedEntry!), First(entries.SecondMovedEntry!));
        ShouldBeTouchCommand(lines[4], Second(entries.SecondTouchedEntry!), First(entries.FirstTouchedEntry!));
        ShouldBeDeleteCommand(lines[5], First(entries.DeletedEntry!));
    }

    [Theory]
    [InlineData("abc def", "'abc def'")]
    [InlineData("\"abc def\"", "'\"abc def\"'")]
    [InlineData("apos' test", "'apos'\\'' test'")]
    [InlineData("abcd\\ef", "'abcd\\ef'")]
    public async Task Write_Escaped_Paths(string path, string expected)
    {
        var directorySeparator = '/';
        var firstSnapshot = new DirMetaSnapshot(directorySeparator);
        var secondSnapshot = new DirMetaSnapshot(directorySeparator);

        firstSnapshot.AddEntry(new DirMetaSnapshotEntryBuilder()
            .WithPath(path)
            .Build());

        var diff = new DirMetaSnapshotComparer().Compare(firstSnapshot, secondSnapshot);

        var diffWriter = new DirMetaSnapshotDiffBashWriter();
        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        lines.Length.ShouldBe(2);
        lines[0].EndsWith(expected).ShouldBeTrue($"Line does not end with '{expected}': {lines[0]}");
    }

    private static void ShouldBeCreateCommand(string command, string reference, string subject)
    {
        command.ShouldBe($"cp -- '{reference}' '{subject}'");
    }

    private static void ShouldBeModifyCommand(string command, string reference, string subject)
    {
        command.ShouldBe($"cp -- '{reference}' '{subject}'");
    }

    private static void ShouldBeCopyCommand(string command, string reference, string subject)
    {
        command.ShouldBe($"cp -- '{reference}' '{subject}'");
    }

    private static void ShouldBeMoveCommand(string command, string oldPath, string newPath)
    {
        command.ShouldBe($"mv -- '{oldPath}' '{newPath}'");
    }

    private static void ShouldBeTouchCommand(string command, string reference, string subject)
    {
        command.ShouldBe($"touch -d \"$(stat -c %y -- '{reference}')\" -- '{subject}'");
    }

    private static void ShouldBeDeleteCommand(string command, string path)
    {
        command.ShouldBe($"rm -- '{path}'");
    }
}
