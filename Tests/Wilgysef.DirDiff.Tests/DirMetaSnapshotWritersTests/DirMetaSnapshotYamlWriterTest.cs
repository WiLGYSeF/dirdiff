using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using Wilgysef.DirDiff.Extensions;
using Wilgysef.DirDiff.Tests.Utils;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Wilgysef.DirDiff.Tests.DirMetaSnapshotWritersTests;

public class DirMetaSnapshotYamlWriterTest
{
    [Fact]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var snapshot = new DirMetaSnapshot(directorySeparator);
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = factory.Create().Build();
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
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.DirectorySeparator.ShouldBe(directorySeparator);
        result.Prefix.ShouldBe(snapshot.Prefix);
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
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var snapshot = new DirMetaSnapshot(directorySeparator);
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = factory.Create()
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
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.DirectorySeparator.ShouldBe(directorySeparator);
        result.Prefix.ShouldBe(snapshot.Prefix);
        result.Entries!.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e.Path == entry.Path);
        }
    }

    [Fact]
    public async Task Write_Hash_LastModifiedTime_FileSize()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var snapshot = new DirMetaSnapshot(directorySeparator);
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = factory.Create().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.DirectorySeparator.ShouldBe(directorySeparator);
        result.Prefix.ShouldBe(snapshot.Prefix);
        result.Entries!.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var resultEntry = result.Entries.Single(e => e.Path == entry.Path);

            resultEntry.Hash.ShouldBe(entry.HashHex);
            resultEntry.LastModifiedTime.ShouldBe(entry.LastModifiedTime!.Value);
            resultEntry.FileSize.ShouldBe(entry.FileSize!.Value);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Write_Prefix(bool writePrefix)
    {
        var directorySeparator = '/';
        var prefix = "abc/";

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var snapshot = new DirMetaSnapshot(directorySeparator);
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = factory.Create()
                .WithRandomPath(prefix)
                .Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        snapshot.Prefix.ShouldBe(prefix);

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
                options.WritePrefix = writePrefix;
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.DirectorySeparator.ShouldBe(directorySeparator);
        result.Prefix.ShouldBe(writePrefix ? snapshot.Prefix : null);
        result.Entries!.Count.ShouldBe(entries.Count);

        foreach (var entry in entries)
        {
            var path = writePrefix ? entry.Path : snapshot.PathWithoutPrefix(entry.Path);
            var resultEntry = result.Entries.Single(e => e.Path == path);
        }
    }

    [Fact]
    public async Task Write_SortByPath()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var snapshot = new DirMetaSnapshot(directorySeparator);
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = factory.Create().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.SortByPath = true;
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var result = DeserializeSnapshot(Encoding.UTF8.GetString(stream.ToArray()));

        result.Entries!.Select(e => e.Path).ToList()
            .ShouldBeEquivalentTo(entries.OrderBy(e => e.Path).Select(e => e.Path).ToList());
    }

    private static DirMetaSnapshotSchema DeserializeSnapshot(string text)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var result = deserializer.Deserialize<DirMetaSnapshotSchema>(text)
            ?? throw new ArgumentException("Text could not be deserialized to snapshot.", nameof(text));
        return result;
    }
}
