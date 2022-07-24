using DirDiff.DirMetaSnapshotComparers;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffYamlWriter : IDirMetaSnapshotDiffWriter
{
    public DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        var schema = DiffSchemaSerializer.SerializeDiff(diff, Options);

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(serializer.Serialize(schema)));
    }
}
