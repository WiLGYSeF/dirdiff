using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotJsonWriter : IDirMetaSnapshotWriter
{
    /// <summary>
    /// Snapshot writer options.
    /// </summary>
    public DirMetaSnapshotJsonWriterOptions JsonWriterOptions { get; } = new();

    public DirMetaSnapshotWriterOptions Options => JsonWriterOptions;

    /// <summary>
    /// Configures snapshot writer options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    public DirMetaSnapshotJsonWriter Configure(Action<DirMetaSnapshotJsonWriterOptions> action)
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
        var schema = new
        {
            Entries = snapshot.Entries
                .Where(e => e.Type != FileType.Directory)
                .Select(e => SerializeEntry(snapshot, e)),
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = JsonWriterOptions.WriteIndented,

        };
        options.Converters.Add(new JsonStringEnumConverter());
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(schema, options));
    }

    private Dictionary<string, object> SerializeEntry(DirMetaSnapshot snapshot, DirMetaSnapshotEntry entry)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "path", Options.WritePrefix ? entry.Path : snapshot.PathWithoutPrefix(entry.Path) },
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
                ? ((DateTimeOffset)entry.CreatedTime.Value).ToUnixTimeSeconds()
                : entry.CreatedTime.Value;
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = JsonWriterOptions.UseUnixTimestamp
                ? ((DateTimeOffset)entry.LastModifiedTime.Value).ToUnixTimeSeconds()
                : entry.LastModifiedTime.Value;
        }

        if (Options.WriteFileSize && entry.FileSize.HasValue)
        {
            dictionary["fileSize"] = entry.FileSize.Value;
        }

        return dictionary;
    }
}
