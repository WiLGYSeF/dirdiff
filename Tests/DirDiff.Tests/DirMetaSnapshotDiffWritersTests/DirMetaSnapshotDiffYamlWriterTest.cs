using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.Tests.DirMetaSnapshotDiffWritersTests;

public class DirMetaSnapshotDiffYamlWriterTest
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

        var diffWriter = new DirMetaSnapshotDiffYamlWriter();
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

    private static Task<TestHelper.DiffSchema> DeserializeDiffAsync(Stream stream)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using var reader = new StreamReader(stream);
        var result = deserializer.Deserialize<TestHelper.DiffSchema>(reader);

        if (result == null)
        {
            throw new ArgumentException("Stream could not be deserialized to diff.", nameof(stream));
        }
        return Task.FromResult(result);
    }
}
