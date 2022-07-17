using DirDiff.DirMetaSnapshots;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffJsonWriter : IDirMetaSnapshotDiffWriter
{
    public DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        var json = new
        {
            Created = diff.CreatedEntries.Select(e => MapEntry(diff, e)),
            Deleted = diff.DeletedEntries.Select(e => MapEntry(diff, e)),
            Modified = diff.ModifiedEntries.Select(p => MapEntryPair(diff, p)),
            Copied = diff.CopiedEntries.Select(p => MapEntryPair(diff, p)),
            Moved = diff.MovedEntries.Select(p => MapEntryPair(diff, p)),
            Touched = diff.TouchedEntries.Select(p => MapEntryPair(diff, p)),
            Unchanged = diff.UnchangedEntries.Select(e => MapEntry(diff, e)),
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(json, options));
    }

    private object MapEntry(DirMetaSnapshotDiff diff, DirMetaSnapshotEntry entry)
    {
        return new
        {
            Path = Options.WritePrefix ? diff.GetEntryPathWithoutPrefix(entry) : entry.Path,
            entry.Type,
            entry.FileSize,
            entry.CreatedTime,
            entry.LastModifiedTime,
            entry.HashAlgorithm,
            Hash = entry.HashHex,
        };
    }

    private object MapEntryPair(DirMetaSnapshotDiff diff, DirMetaSnapshotDiffEntryPair pair)
    {
        return new
        {
            First = MapEntry(diff, pair.First),
            Second = MapEntry(diff, pair.Second),
        };
    }
}
