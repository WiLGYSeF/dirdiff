using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Extensions;
using DirDiff.Tests.Utils;
using System.Text;
using System.Text.Json;

namespace DirDiff.Tests.DirMetaSnapshotWritersTests;

public class DirMetaSnapshotJsonWriterTest
{
    [Fact]
    public void Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotJsonWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
                options.UseUnixTimestamp = false;
            });

        writer.Write(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e["path"].ToString() == entry.Path);

            resultEntry["hash"].GetString().ShouldBe(entry.HashHex);
            resultEntry["hashAlgorithm"].GetString().ShouldBe(entry.HashAlgorithm!.Value.ToEnumMemberValue());
            resultEntry["createdTime"].GetDateTime().ShouldBe(entry.CreatedTime!.Value);
            resultEntry["lastModifiedTime"].GetDateTime().ShouldBe(entry.LastModifiedTime!.Value);
            resultEntry["fileSize"].GetInt64().ShouldBe(entry.FileSize!.Value);
        }
    }

    [Fact]
    public void Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize_NoValue()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithFileSize(null)
                .Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotJsonWriter()
            .Configure(options =>
            {
                options.WriteHash = false;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = false;
                options.WriteFileSize = false;
                options.UseUnixTimestamp = false;
            });

        writer.Write(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e["path"].ToString() == entry.Path);

            resultEntry.Count.ShouldBe(2);
        }
    }

    [Fact]
    public void Write_Hash_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotJsonWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
                options.UseUnixTimestamp = false;
            });

        writer.Write(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e["path"].ToString() == entry.Path);

            resultEntry["hash"].GetString().ShouldBe(entry.HashHex);
            resultEntry["lastModifiedTime"].GetDateTime().ShouldBe(entry.LastModifiedTime!.Value);
            resultEntry["fileSize"].GetInt64().ShouldBe(entry.FileSize!.Value);
        }
    }

    private static SnapshotDeserialized DeserializeSnapshot(string text)
    {
        var result = JsonSerializer.Deserialize<SnapshotDeserialized>(
            text,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

        if (result == null)
        {
            throw new ArgumentException("Text could not be deserialized to snapshot.", nameof(text));
        }

        return result;
    }

    private class SnapshotDeserialized
    {
        public List<Dictionary<string, JsonElement>>? Entries { get; set; }
    }
}
