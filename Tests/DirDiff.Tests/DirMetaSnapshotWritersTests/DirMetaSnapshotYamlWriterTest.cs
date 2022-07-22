using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Extensions;
using DirDiff.Tests.Utils;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.Tests.DirMetaSnapshotWritersTests;

public class DirMetaSnapshotYamlWriterTest
{
    [Fact]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
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

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
                options.UseUnixTimestamp = false;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries!.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e.Path == entry.Path);

            resultEntry.Hash.ShouldBe(entry.HashHex);
            resultEntry.HashAlgorithm.ShouldBe(entry.HashAlgorithm!.Value.ToEnumMemberValue());
            resultEntry.CreatedTime.ShouldBe(entry.CreatedTime!.Value);
            resultEntry.LastModifiedTime.ShouldBe(entry.LastModifiedTime!.Value);
            resultEntry.FileSize.ShouldBe(entry.FileSize!.Value);
        }
    }

    [Fact]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize_NoValue()
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

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.WriteHash = false;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = false;
                options.WriteFileSize = false;
                options.UseUnixTimestamp = false;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries!.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e.Path == entry.Path);
        }
    }

    [Fact]
    public async Task Write_Hash_LastModifiedTime_FileSize()
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

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
                options.UseUnixTimestamp = false;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries!.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e.Path == entry.Path);

            resultEntry.Hash.ShouldBe(entry.HashHex);
            resultEntry.LastModifiedTime.ShouldBe(entry.LastModifiedTime!.Value);
            resultEntry.FileSize.ShouldBe(entry.FileSize!.Value);
        }
    }

    private static DirMetaSnapshotSchema DeserializeSnapshot(string text)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var result = deserializer.Deserialize<DirMetaSnapshotSchema>(text);

        if (result == null)
        {
            throw new ArgumentException("Text could not be deserialized to snapshot.", nameof(text));
        }

        return result;
    }
}
