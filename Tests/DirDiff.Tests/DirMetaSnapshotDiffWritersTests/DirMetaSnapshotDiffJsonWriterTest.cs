using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshots;
using System.Text.Json;

namespace DirDiff.Tests.DirMetaSnapshotDiffWritersTests;

public class DirMetaSnapshotDiffJsonWriterTest
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

        var diffWriter = new DirMetaSnapshotDiffJsonWriter();
        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = await DeserializeDiffAsync(stream);

        result.Created!.Single().Path.ShouldBe(entries.CreatedEntry!.Path);
        result.Deleted!.Single().Path.ShouldBe(entries.DeletedEntry!.Path);
        result.Modified!.Single().First!.Path.ShouldBe(entries.FirstModifiedEntry!.Path);
        result.Modified!.Single().Second!.Path.ShouldBe(entries.SecondModifiedEntry!.Path);
        result.Copied!.Single().First!.Path.ShouldBe(entries.FirstCopiedEntry!.Path);
        result.Copied!.Single().Second!.Path.ShouldBe(entries.SecondCopiedEntry!.Path);
        result.Moved!.Single().First!.Path.ShouldBe(entries.FirstMovedEntry!.Path);
        result.Moved!.Single().Second!.Path.ShouldBe(entries.SecondMovedEntry!.Path);
        result.Touched!.Single().First!.Path.ShouldBe(entries.FirstTouchedEntry!.Path);
        result.Touched!.Single().Second!.Path.ShouldBe(entries.SecondTouchedEntry!.Path);
        result.Unchanged!.Single().Path.ShouldBe(entries.SecondUnchangedEntry!.Path);
    }

    [Fact]
    public async Task Write_No_Prefix()
    {
        var directorySeparator = '/';
        var firstPrefix = "test/";
        var secondPrefix = "abc/";

        var firstSnapshot = new DirMetaSnapshot(directorySeparator);
        var secondSnapshot = new DirMetaSnapshot(directorySeparator);

        var entries = TestHelper.SetUpBasicDiff(firstSnapshot, secondSnapshot, firstPrefix, secondPrefix);

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffJsonWriter().Configure(options =>
        {
            options.WritePrefix = false;
        });

        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = await DeserializeDiffAsync(stream);

        result.Created!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.CreatedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Deleted!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.DeletedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Modified!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstModifiedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Modified!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondModifiedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Copied!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstCopiedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Copied!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondCopiedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Moved!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstMovedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Moved!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondMovedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Touched!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstTouchedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Touched!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondTouchedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
        result.Unchanged!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondUnchangedEntry!, firstSnapshot, secondSnapshot, null, "", ""));
    }

    [Theory]
    [InlineData(null)]
    [InlineData('/')]
    public async Task Write_Different_DirectorySeparator(char? writerDirectorySeparator)
    {
        var firstDirectorySeparator = '/';
        var secondDirectorySeparator = '\\';
        var firstPrefix = "test/";
        var secondPrefix = "abc\\";

        var firstSnapshot = new DirMetaSnapshot(firstDirectorySeparator);
        var secondSnapshot = new DirMetaSnapshot(secondDirectorySeparator);

        var entries = TestHelper.SetUpBasicDiff(firstSnapshot, secondSnapshot, firstPrefix, secondPrefix);

        var diff = secondSnapshot.Compare(firstSnapshot);

        var diffWriter = new DirMetaSnapshotDiffJsonWriter().Configure(options =>
        {
            options.DirectorySeparator = writerDirectorySeparator;
        });

        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = await DeserializeDiffAsync(stream);

        result.Created!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.CreatedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Deleted!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.DeletedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Modified!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstModifiedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Modified!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondModifiedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Copied!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstCopiedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Copied!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondCopiedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Moved!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstMovedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Moved!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondMovedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Touched!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstTouchedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Touched!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondTouchedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
        result.Unchanged!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondUnchangedEntry!, firstSnapshot, secondSnapshot, writerDirectorySeparator));
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

        var diffWriter = new DirMetaSnapshotDiffJsonWriter().Configure(options =>
        {
            options.FirstPrefix = firstNewPrefix;
            options.SecondPrefix = secondNewPrefix;
        });

        var stream = new MemoryStream();
        await diffWriter.WriteAsync(stream, diff);
        stream.Position = 0;

        var result = await DeserializeDiffAsync(stream);

        result.Created!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.CreatedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Deleted!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.DeletedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Modified!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstModifiedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Modified!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondModifiedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Copied!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstCopiedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Copied!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondCopiedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Moved!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstMovedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Moved!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondMovedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Touched!.Single().First!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.FirstTouchedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Touched!.Single().Second!.Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondTouchedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
        result.Unchanged!.Single().Path
            .ShouldBe(TestHelper.GetEntryPath(entries.SecondUnchangedEntry!, firstSnapshot, secondSnapshot, null, firstNewPrefix, secondNewPrefix));
    }

    private static async Task<TestHelper.DiffSchema> DeserializeDiffAsync(Stream stream)
    {
        var result = await JsonSerializer.DeserializeAsync<TestHelper.DiffSchema>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
        if (result == null)
        {
            throw new ArgumentException("Stream could not be deserialized to diff.", nameof(stream));
        }
        return result;
    }
}
