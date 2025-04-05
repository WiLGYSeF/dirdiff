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

    public Task WriteAsync(Stream stream, DirMetaSnapshot snapshot)
    {
        var options = new JsonWriterOptions
        {
            Indented = JsonWriterOptions.WriteIndented,
        };

        using var jsonWriter = new Utf8JsonWriter(stream, options);

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotSchema.DirectorySeparator)), (JsonWriterOptions.DirectorySeparator ?? snapshot.DirectorySeparator).ToString());

        if (Options.WritePrefix)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotSchema.Prefix)), snapshot.Prefix);
        }

        var entries = snapshot.Entries.Where(e => e.Type != FileType.Directory);

        if (Options.SortByPath)
        {
            entries = entries.OrderBy(e => e.Path);
        }

        jsonWriter.WriteStartArray(ToCamelCase(nameof(DirMetaSnapshotSchema.Entries)));

        foreach (var entry in entries)
        {
            SerializeEntry(snapshot, entry, jsonWriter);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        return Task.CompletedTask;
    }

    private void SerializeEntry(DirMetaSnapshot snapshot, DirMetaSnapshotEntry entry, Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject();

        var path = Options.WritePrefix
            ? entry.Path
            : snapshot.PathWithoutPrefix(entry.Path);

        if (Options.DirectorySeparator.HasValue && Options.DirectorySeparator.Value != snapshot.DirectorySeparator)
        {
            path = snapshot.ChangePathDirectorySeparator(path, Options.DirectorySeparator.Value);
        }

        jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Path)), path);

        if (Options.WriteType)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Type)), entry.Type.ToString());
        }

        if (Options.WriteHash && entry.Hash != null)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Hash)), entry.HashHex);

            if (Options.WriteHashAlgorithm && entry.HashAlgorithm.HasValue)
            {
                jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.HashAlgorithm)), entry.HashAlgorithm.Value.ToEnumMemberValue());
            }
        }

        if (Options.WriteCreatedTime && entry.CreatedTime.HasValue)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.CreatedTime)), entry.CreatedTime.Value.ToString("o"));
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.LastModifiedTime)), entry.LastModifiedTime.Value.ToString("o"));
        }

        if (Options.WriteFileSize && entry.FileSize.HasValue)
        {
            jsonWriter.WriteNumber(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.FileSize)), entry.FileSize.Value);
        }

        jsonWriter.WriteEndObject();
    }

    private static string ToCamelCase(string a)
    {
        return a[..1].ToLower() + a[1..];
    }
}
