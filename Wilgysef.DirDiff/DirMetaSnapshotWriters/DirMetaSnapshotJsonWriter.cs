using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wilgysef.DirDiff.DirMetaSnapshotWriters;

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
        var schema = new Dictionary<string, object>
        {
            [ToCamelCase(nameof(DirMetaSnapshotSchema.DirectorySeparator))] = JsonWriterOptions.DirectorySeparator ?? snapshot.DirectorySeparator,
        };
        var entries = snapshot.Entries.Where(e => e.Type != FileType.Directory);

        if (Options.WritePrefix)
        {
            schema[ToCamelCase(nameof(DirMetaSnapshotSchema.Prefix))] = snapshot.Prefix!;
        }

        if (Options.SortByPath)
        {
            entries = entries.OrderBy(e => e.Path);
        }

        schema[ToCamelCase(nameof(DirMetaSnapshotSchema.Entries))] = entries.Select(e => SerializeEntry(snapshot, e));

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
        var path = Options.WritePrefix
            ? entry.Path
            : snapshot.PathWithoutPrefix(entry.Path);

        if (Options.DirectorySeparator.HasValue && Options.DirectorySeparator.Value != snapshot.DirectorySeparator)
        {
            path = snapshot.ChangePathDirectorySeparator(path, Options.DirectorySeparator.Value);
        }

        var dictionary = new Dictionary<string, object>
        {
            [ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Path))] = path,
            [ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Type))] = entry.Type,
        };

        if (Options.WriteHash && entry.Hash != null)
        {
            dictionary[ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Hash))] = entry.HashHex!;

            if (Options.WriteHashAlgorithm && entry.HashAlgorithm.HasValue)
            {
                dictionary[ToCamelCase(nameof(DirMetaSnapshotEntrySchema.HashAlgorithm))] = entry.HashAlgorithm.Value.ToEnumMemberValue();
            }
        }

        if (Options.WriteCreatedTime && entry.CreatedTime.HasValue)
        {
            dictionary[ToCamelCase(nameof(DirMetaSnapshotEntrySchema.CreatedTime))] = entry.CreatedTime.Value;
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            dictionary[ToCamelCase(nameof(DirMetaSnapshotEntrySchema.LastModifiedTime))] = entry.LastModifiedTime.Value;
        }

        if (Options.WriteFileSize && entry.FileSize.HasValue)
        {
            dictionary[ToCamelCase(nameof(DirMetaSnapshotEntrySchema.FileSize))] = entry.FileSize.Value;
        }

        return dictionary;
    }

    private static string ToCamelCase(string a)
    {
        return a[..1].ToLower() + a[1..];
    }
}
