using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotYamlWriter : IDirMetaSnapshotWriter
{
    /// <summary>
    /// Snapshot writer options.
    /// </summary>
    public DirMetaSnapshotYamlWriterOptions YamlWriterOptions { get; } = new();

    public DirMetaSnapshotWriterOptions Options => YamlWriterOptions;

    /// <summary>
    /// Configures snapshot writer options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    public DirMetaSnapshotYamlWriter Configure(Action<DirMetaSnapshotYamlWriterOptions> action)
    {
        action(YamlWriterOptions);
        return this;
    }

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action)
    {
        action(YamlWriterOptions);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshot snapshot)
    {
        var schema = new DirMetaSnapshotSchema
        {
            Entries = snapshot.Entries
                .Where(e => e.Type != FileType.Directory)
                .Select(e => SerializeEntry(snapshot, e)),
        };
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(serializer.Serialize(schema)));
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
            dictionary["createdTime"] = YamlWriterOptions.UseUnixTimestamp
                ? ((DateTimeOffset)entry.CreatedTime.Value).ToUnixTimeSeconds()
                : entry.CreatedTime.Value;
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = YamlWriterOptions.UseUnixTimestamp
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
