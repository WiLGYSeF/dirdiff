using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotJsonWriter : IDirMetaSnapshotWriter
{
    public DirMetaSnapshotJsonWriterOptions JsonWriterOptions { get; } = new();

    public DirMetaSnapshotWriterOptions Options => JsonWriterOptions;

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotJsonWriterOptions> action)
    {
        action(JsonWriterOptions);
        return this;
    }

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action)
    {
        action(JsonWriterOptions);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshot snapshot)
    {
        var json = new DirMetaSnapshotJsonSchema
        {
            Entries = snapshot.Entries
                .Where(e => e.Type != FileType.Directory)
                .Select(e => SerializeEntry(e)),
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = JsonWriterOptions.WriteIndented,

        };
        options.Converters.Add(new JsonStringEnumConverter());
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(json, options));
    }

    private Dictionary<string, object> SerializeEntry(DirMetaSnapshotEntry entry)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "path", entry.Path },
            { "type", entry.Type },
        };

        if (Options.WriteHash && entry.Hash != null)
        {
            dictionary["hash"] = entry.HashHex!;

            if (Options.WriteHashAlgorithm && entry.HashAlgorithm.HasValue)
            {
                dictionary["hashAlgorithm"] = entry.HashAlgorithm.Value.ToEnumMemberValue();
            }
        }

        if (Options.WriteCreatedTime && entry.CreatedTime.HasValue)
        {
            dictionary["createdTime"] = JsonWriterOptions.UseUnixTimestamp
                ? entry.CreatedTime.Value.ToUnixTimestamp()
                : entry.CreatedTime.Value;
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = JsonWriterOptions.UseUnixTimestamp
                ? entry.LastModifiedTime.Value.ToUnixTimestamp()
                : entry.LastModifiedTime.Value;
        }

        if (Options.WriteFileSize && entry.FileSize.HasValue)
        {
            dictionary["fileSize"] = entry.FileSize.Value;
        }

        return dictionary;
    }
}
