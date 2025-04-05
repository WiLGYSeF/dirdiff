using Wilgysef.DirDiff.DirMetaSnapshotComparers;
using System.Text.Json;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using Wilgysef.DirDiff.Extensions;

namespace Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;

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

    public Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        using var jsonWriter = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = JsonWriterOptions.WriteIndented,
        });

        jsonWriter.WriteStartObject();

        jsonWriter.WriteStartArray("created");

        foreach (var entry in diff.CreatedEntries)
        {
            WriteEntry(diff, entry, Options.SecondPrefix, Options, null, jsonWriter);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteStartArray("deleted");

        foreach (var entry in diff.DeletedEntries)
        {
            WriteEntry(diff, entry, Options.FirstPrefix, Options, null, jsonWriter);
        }

        jsonWriter.WriteEndArray();

        WriteEntryPairArray("modified", diff.ModifiedEntries);
        WriteEntryPairArray("copied", diff.CopiedEntries);
        WriteEntryPairArray("moved", diff.MovedEntries);
        WriteEntryPairArray("touched", diff.TouchedEntries);

        if (Options.WriteUnchanged)
        {
            jsonWriter.WriteStartArray("unchanged");

            foreach (var entry in diff.UnchangedEntries)
            {
                WriteEntry(diff, entry, Options.SecondPrefix, Options, null, jsonWriter);
            }

            jsonWriter.WriteEndArray();
        }

        jsonWriter.WriteEndObject();
        return Task.CompletedTask;

        void WriteEntryPairArray(string propertyName, IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> pairs)
        {
            jsonWriter.WriteStartArray(propertyName);

            foreach (var pair in pairs)
            {
                WriteEntryPair(diff, pair, Options, jsonWriter);
            }

            jsonWriter.WriteEndArray();
        }
    }

    private static void WriteEntryPair(
        DirMetaSnapshotDiff diff,
        DirMetaSnapshotDiffEntryPair pair,
        DirMetaSnapshotDiffWriterOptions options,
        Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject();

        WriteEntry(diff, pair.First, options.FirstPrefix, options, "first", jsonWriter);
        WriteEntry(diff, pair.Second, options.SecondPrefix, options, "second", jsonWriter);

        jsonWriter.WriteEndObject();
    }

    private static void WriteEntry(
        DirMetaSnapshotDiff diff,
        DirMetaSnapshotEntry entry,
        string? prefix,
        DirMetaSnapshotDiffWriterOptions options,
        string? propertyName,
        Utf8JsonWriter jsonWriter)
    {
        if (propertyName != null)
        {
            jsonWriter.WriteStartObject(propertyName);
        }
        else
        {
            jsonWriter.WriteStartObject();
        }

        var snapshot = diff.GetEntrySnapshot(entry);

        var path = prefix != null
            ? prefix + snapshot.PathWithoutPrefix(entry.Path)
            : entry.Path;
        if (options.DirectorySeparator.HasValue)
        {
            path = snapshot.ChangePathDirectorySeparator(path, options.DirectorySeparator.Value);
        }

        jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Path)), path);

        if (options.WriteType)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Type)), entry.Type.ToString());
        }

        if (entry.Hash != null)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Hash)), entry.HashHex);

            if (entry.HashAlgorithm.HasValue)
            {
                jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.HashAlgorithm)), entry.HashAlgorithm.Value.ToEnumMemberValue());
            }
        }

        if (entry.CreatedTime.HasValue)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.CreatedTime)), entry.CreatedTime.Value.ToString("o"));
        }

        if (entry.LastModifiedTime.HasValue)
        {
            jsonWriter.WriteString(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.LastModifiedTime)), entry.LastModifiedTime.Value.ToString("o"));
        }

        if (entry.FileSize.HasValue)
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
