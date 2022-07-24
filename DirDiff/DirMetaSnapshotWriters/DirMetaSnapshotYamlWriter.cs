using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotYamlWriter : IDirMetaSnapshotWriter
{
    public DirMetaSnapshotWriterOptions Options { get; } = new();

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action)
    {
        action(Options);
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
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(serializer.Serialize(schema)));
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
            { "path", path },
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
            dictionary["createdTime"] = entry.CreatedTime.Value;
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = entry.LastModifiedTime.Value;
        }

        if (Options.WriteFileSize && entry.FileSize.HasValue)
        {
            dictionary["fileSize"] = entry.FileSize.Value;
        }

        return dictionary;
    }
}
