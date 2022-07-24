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
