using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using Wilgysef.DirDiff.Utilities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Wilgysef.DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotYamlReader : IDirMetaSnapshotReader
{
    public DirMetaSnapshotReaderOptions Options { get; } = new();

    public IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action)
    {
        action(Options);
        return this;
    }

    public Task<DirMetaSnapshot> ReadAsync(Stream stream)
    {
        var result = DeserializeSnapshotAsync(stream);
        var directorySeparator = result.DirectorySeparator
            ?? PathUtils.GuessDirectorySeparator(result.Entries!.Where(e => e.Path != null).Select(e => e.Path!));

        var snapshot = new DirMetaSnapshot(directorySeparator);

        foreach (var entry in result.Entries!)
        {
            snapshot.AddEntry(entry.ToEntry());
        }

        return Task.FromResult(snapshot);
    }

    private static DirMetaSnapshotSchema DeserializeSnapshotAsync(Stream stream)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using var reader = new StreamReader(stream);
        var result = deserializer.Deserialize<DirMetaSnapshotSchema>(reader);

        if (result == null)
        {
            throw new ArgumentException("Stream could not be deserialized to snapshot.", nameof(stream));
        }
        return result;
    }
}
