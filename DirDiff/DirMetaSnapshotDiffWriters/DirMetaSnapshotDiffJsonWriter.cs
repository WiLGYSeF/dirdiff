using DirDiff.DirMetaSnapshots;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffJsonWriter : IDirMetaSnapshotDiffWriter
{
    public async Task Write(Stream stream, DirMetaSnapshotDiff diff)
    {
        var json = new
        {
            Created = diff.CreatedEntries,
            Deleted = diff.DeletedEntries,
            Modified = diff.ModifiedEntries,
            Moved = diff.MovedEntries,
            Touched = diff.TouchedEntries,
            Unchanged = diff.UnchangedEntries,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(json, options));
    }
}
