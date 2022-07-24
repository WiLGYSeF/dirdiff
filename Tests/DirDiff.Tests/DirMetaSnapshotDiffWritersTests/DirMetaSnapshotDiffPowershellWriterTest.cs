using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshots;
using DirDiff.Tests.Utils;
using System.Text;

namespace DirDiff.Tests.DirMetaSnapshotDiffWritersTests;

public class DirMetaSnapshotDiffPowershellWriterTest
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

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffPowershellWriter();
        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        lines.Length.ShouldBe(7);
        ShouldBeCreateCommand(lines[0], entries.CreatedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeModifyCommand(lines[1], entries.FirstModifiedEntry!, entries.SecondModifiedEntry!);
        ShouldBeCopyCommand(lines[2], entries.FirstCopiedEntry!, entries.SecondCopiedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeMoveCommand(lines[3], entries.FirstMovedEntry!, entries.SecondMovedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeTouchCommand(lines[4], entries.FirstTouchedEntry!, entries.SecondTouchedEntry!);
        ShouldBeDeleteCommand(lines[5], entries.DeletedEntry!);
    }

    [Fact]
    public async Task Write_Different_DirectorySeparator()
    {
        var firstDirectorySeparator = '/';
        var secondDirectorySeparator = '\\';
        var firstPrefix = "test/";
        var secondPrefix = "abc\\";

        var firstSnapshot = new DirMetaSnapshot(firstDirectorySeparator);
        var secondSnapshot = new DirMetaSnapshot(secondDirectorySeparator);

        var entries = TestHelper.SetUpBasicDiff(firstSnapshot, secondSnapshot, firstPrefix, secondPrefix);

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffPowershellWriter();
        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        lines.Length.ShouldBe(7);
        ShouldBeCreateCommand(lines[0], entries.CreatedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeModifyCommand(lines[1], entries.FirstModifiedEntry!, entries.SecondModifiedEntry!);
        ShouldBeCopyCommand(lines[2], entries.FirstCopiedEntry!, entries.SecondCopiedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeMoveCommand(lines[3], entries.FirstMovedEntry!, entries.SecondMovedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeTouchCommand(lines[4], entries.FirstTouchedEntry!, entries.SecondTouchedEntry!);
        ShouldBeDeleteCommand(lines[5], entries.DeletedEntry!);
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

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffPowershellWriter().Configure(options =>
        {
            options.FirstPrefix = firstNewPrefix;
            options.SecondPrefix = secondNewPrefix;
        });

        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        lines.Length.ShouldBe(7);
        ShouldBeCreateCommand(lines[0], entries.CreatedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeModifyCommand(lines[1], entries.FirstModifiedEntry!, entries.SecondModifiedEntry!);
        ShouldBeCopyCommand(lines[2], entries.FirstCopiedEntry!, entries.SecondCopiedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeMoveCommand(lines[3], entries.FirstMovedEntry!, entries.SecondMovedEntry!, firstSnapshot, secondSnapshot);
        ShouldBeTouchCommand(lines[4], entries.FirstTouchedEntry!, entries.SecondTouchedEntry!);
        ShouldBeDeleteCommand(lines[5], entries.DeletedEntry!);
    }

    [Theory]
    [InlineData("abc def", "\"abc def\"")]
    [InlineData("abcd\"ef", "\"abcd`\"ef\"")]
    public async Task Write_Escaped_Paths(string path, string expected)
    {
        var firstSnapshot = new DirMetaSnapshot();
        var secondSnapshot = new DirMetaSnapshot();

        firstSnapshot.AddEntry(new DirMetaSnapshotEntryBuilder()
            .WithPath(path)
            .Build());

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffPowershellWriter();
        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = Encoding.UTF8.GetString(stream.ToArray());

        var lines = result.Split(Environment.NewLine);

        lines.Length.ShouldBe(2);
        lines[0].EndsWith(expected).ShouldBeTrue($"Line does not end with '{expected}': {lines[0]}");
    }

    private static void ShouldBeCreateCommand(string command, DirMetaSnapshotEntry entry, DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        command.ShouldBe($"Copy-Item -LiteralPath \"{entry.Path}\" -Destination \"{firstSnapshot.Prefix + secondSnapshot.PathWithoutPrefix(entry.Path)}\"");
    }

    private static void ShouldBeModifyCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry)
    {
        command.ShouldBe($"Copy-Item -LiteralPath \"{secondEntry.Path}\" -Destination \"{firstEntry.Path}\"");
    }

    private static void ShouldBeCopyCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry, DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        command.ShouldBe($"Copy-Item -LiteralPath \"{firstEntry.Path}\" -Destination \"{firstSnapshot.Prefix + secondSnapshot.PathWithoutPrefix(secondEntry.Path)}\"");
    }

    private static void ShouldBeMoveCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry, DirMetaSnapshot firstSnapshot, DirMetaSnapshot secondSnapshot)
    {
        command.ShouldBe($"Move-Item -LiteralPath \"{firstEntry.Path}\" -Destination \"{firstSnapshot.Prefix + secondSnapshot.PathWithoutPrefix(secondEntry.Path)}\"");
    }

    private static void ShouldBeTouchCommand(string command, DirMetaSnapshotEntry firstEntry, DirMetaSnapshotEntry secondEntry)
    {
        command.ShouldBe($"(Get-ChildItem -LiteralPath \"{firstEntry.Path}\").LastWriteTime = (Get-ChildItem -LiteralPath \"{secondEntry.Path}\").LastWriteTime");
    }

    private static void ShouldBeDeleteCommand(string command, DirMetaSnapshotEntry entry)
    {
        command.ShouldBe($"Remove-Item -LiteralPath \"{entry.Path}\"");
    }
}
