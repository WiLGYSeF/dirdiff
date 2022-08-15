using Wilgysef.DirDiff.DirMetaSnapshotComparers;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        var schema = DiffSchemaSerializer.SerializeDiff(diff, Options);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = JsonWriterOptions.WriteIndented,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(schema, options));
    }
}
