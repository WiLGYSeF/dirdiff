using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Utilities;
using System.Text.Json;

namespace DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotJsonReader : IDirMetaSnapshotReader
{
    public DirMetaSnapshotReaderOptions Options { get; } = new();

    private static readonly string PathKey = "path";
    private static readonly string TypeKey = "type";
    private static readonly string FileSizeKey = "fileSize";
    private static readonly string CreatedTimeKey = "createdTime";
    private static readonly string LastModifiedTimeKey = "lastModifiedTime";
    private static readonly string HashAlgorithmKey = "hashAlgorithm";
    private static readonly string HashKey = "hash";

    public IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task<DirMetaSnapshot> Read(Stream stream)
    {
        var snapshot = new DirMetaSnapshot();

        var result = await DeserializeSnapshot(stream);

        foreach (var resultEntry in result.Entries!)
        {
            var entry = SnapshotEntryFromDictionary(resultEntry);
            snapshot.AddEntry(entry);
        }

        return snapshot;
    }

    private static async Task<DirMetaSnapshotJsonSchema> DeserializeSnapshot(Stream stream)
    {
        var result = await JsonSerializer.DeserializeAsync<DirMetaSnapshotJsonSchema>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
        if (result == null)
        {
            throw new ArgumentException("Stream could not be deserialized to snapshot.", nameof(stream));
        }
        return result;
    }

    private static DirMetaSnapshotEntry SnapshotEntryFromDictionary(IDictionary<string, object> dictionary)
    {
        if (!dictionary.TryGetValue(PathKey, out var path))
        {
            throw new InvalidOperationException($"Entry does not have \"{PathKey}\" value.");
        }
        if (!dictionary.TryGetValue(TypeKey, out var type))
        {
            throw new InvalidOperationException($"Entry does not have \"{TypeKey}\" value.");
        }

        var entry = new DirMetaSnapshotEntry(path.ToString()!, Enum.Parse<FileType>(type.ToString()!));

        if (dictionary.TryGetValueAs(FileSizeKey, out JsonElement fileSize))
        {
            entry.FileSize = fileSize.GetInt64();
        }
        if (dictionary.TryGetValueAs(CreatedTimeKey, out JsonElement createdTime))
        {
            entry.CreatedTime = createdTime.GetDateTime();
        }
        if (dictionary.TryGetValueAs(LastModifiedTimeKey, out JsonElement lastModifiedTime))
        {
            entry.LastModifiedTime = lastModifiedTime.GetDateTime();
        }
        if (dictionary.TryGetValueAs(HashAlgorithmKey, out JsonElement hashAlgorithm))
        {
            entry.HashAlgorithm = EnumUtils.ParseEnumMemberValue<HashAlgorithm>(hashAlgorithm.GetString()!);
        }
        if (dictionary.TryGetValueAs(HashKey, out JsonElement hash))
        {
            entry.Hash = Convert.FromHexString(hash.GetString()!);
        }

        return entry;
    }
}
