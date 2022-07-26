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
        var schema = new Dictionary<string, object>
        {
            [ToCamelCase(nameof(DirMetaSnapshotSchema.DirectorySeparator))] = snapshot.DirectorySeparator,
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
