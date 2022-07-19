using DirDiff.DirMetaSnapshots;
using DirDiff.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffJsonWriter : IDirMetaSnapshotDiffWriter
{
    public DirMetaSnapshotDiffJsonWriterOptions JsonWriterOptions { get; } = new();
    
    public DirMetaSnapshotDiffWriterOptions Options => JsonWriterOptions;

    public DirMetaSnapshotDiffJsonWriter Configure(Action<DirMetaSnapshotDiffJsonWriterOptions> action)
    {
        action(JsonWriterOptions);
        return this;
    }

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(JsonWriterOptions);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        var json = new
        {
            Created = diff.CreatedEntries.Select(e => SerializeEntry(diff, e, Options.SecondPrefix)),
            Deleted = diff.DeletedEntries.Select(e => SerializeEntry(diff, e, Options.FirstPrefix)),
            Modified = diff.ModifiedEntries.Select(p => SerializeEntryPair(diff, p)),
            Copied = diff.CopiedEntries.Select(p => SerializeEntryPair(diff, p)),
            Moved = diff.MovedEntries.Select(p => SerializeEntryPair(diff, p)),
            Touched = diff.TouchedEntries.Select(p => SerializeEntryPair(diff, p)),
            Unchanged = diff.UnchangedEntries.Select(e => SerializeEntry(diff, e, Options.SecondPrefix)),
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = JsonWriterOptions.WriteIndented,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(json, options));
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
            { "path", prefix != null ? prefix + diff.GetEntryPathWithoutPrefix(entry) : entry.Path },
            { "type", entry.Type },
        };

        if ( entry.Hash != null)
        {
            dictionary["hash"] = entry.HashHex!;

            if (entry.HashAlgorithm.HasValue)
            {
                dictionary["hashAlgorithm"] = entry.HashAlgorithm.Value.ToEnumMemberValue();
            }
        }

        if (entry.CreatedTime.HasValue)
        {
            dictionary["createdTime"] = JsonWriterOptions.UseUnixTimestamp
                ? ((DateTimeOffset)entry.CreatedTime.Value).ToUnixTimeSeconds()
                : entry.CreatedTime.Value;
        }

        if (entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = JsonWriterOptions.UseUnixTimestamp
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
