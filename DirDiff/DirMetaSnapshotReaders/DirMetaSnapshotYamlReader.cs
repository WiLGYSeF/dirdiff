﻿using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DirDiff.DirMetaSnapshotReaders;

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
        var snapshot = new DirMetaSnapshot(Options.DirectorySeparator);

        var result = DeserializeSnapshotAsync(stream);

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