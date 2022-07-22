using DirDiff.DirMetaSnapshots;
using DirDiff.Extensions;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffYamlWriter : IDirMetaSnapshotDiffWriter
{
    public DirMetaSnapshotDiffYamlWriterOptions YamlWriterOptions { get; } = new();

    public DirMetaSnapshotDiffWriterOptions Options => YamlWriterOptions;

    public DirMetaSnapshotDiffYamlWriter Configure(Action<DirMetaSnapshotDiffYamlWriterOptions> action)
    {
        action(YamlWriterOptions);
        return this;
    }

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(YamlWriterOptions);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        var schema = new
        {
            Created = diff.CreatedEntries.Select(e => SerializeEntry(diff, e, Options.SecondPrefix)),
            Deleted = diff.DeletedEntries.Select(e => SerializeEntry(diff, e, Options.FirstPrefix)),
            Modified = diff.ModifiedEntries.Select(p => SerializeEntryPair(diff, p)),
            Copied = diff.CopiedEntries.Select(p => SerializeEntryPair(diff, p)),
            Moved = diff.MovedEntries.Select(p => SerializeEntryPair(diff, p)),
            Touched = diff.TouchedEntries.Select(p => SerializeEntryPair(diff, p)),
            Unchanged = diff.UnchangedEntries.Select(e => SerializeEntry(diff, e, Options.SecondPrefix)),
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(serializer.Serialize(schema)));
    }

    private object SerializeEntryPair(DirMetaSnapshotDiff diff, DirMetaSnapshotDiffEntryPair pair)
    {
        return new
        {
            First = SerializeEntry(diff, pair.First, Options.FirstPrefix),
            Second = SerializeEntry(diff, pair.Second, Options.SecondPrefix),
        };
    }

    private Dictionary<string, object> SerializeEntry(DirMetaSnapshotDiff diff, DirMetaSnapshotEntry entry, string? prefix)
    {
        var dictionary = new Dictionary<string, object>
        {
            {
                "path",
                prefix != null
                    ? prefix + diff.GetEntryPathWithoutPrefix(entry)
                    : Options.WritePrefix ? entry.Path : diff.GetEntryPathWithoutPrefix(entry) },
            { "type", entry.Type },
        };

        if (entry.Hash != null)
        {
            dictionary["hash"] = entry.HashHex!;

            if (entry.HashAlgorithm.HasValue)
            {
                dictionary["hashAlgorithm"] = entry.HashAlgorithm.Value.ToEnumMemberValue();
            }
        }

        if (entry.CreatedTime.HasValue)
        {
            dictionary["createdTime"] = YamlWriterOptions.UseUnixTimestamp
                ? ((DateTimeOffset)entry.CreatedTime.Value).ToUnixTimeSeconds()
                : entry.CreatedTime.Value;
        }

        if (entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = YamlWriterOptions.UseUnixTimestamp
                ? ((DateTimeOffset)entry.LastModifiedTime.Value).ToUnixTimeSeconds()
                : entry.LastModifiedTime.Value;
        }

        if (entry.FileSize.HasValue)
        {
            dictionary["fileSize"] = entry.FileSize.Value;
        }

        return dictionary;
    }
}
