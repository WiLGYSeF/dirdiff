﻿using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using System.Text.Json;

namespace DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotJsonReader : IDirMetaSnapshotReader
{
    public DirMetaSnapshotReaderOptions Options { get; } = new();

    public IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task<DirMetaSnapshot> ReadAsync(Stream stream)
    {
        var snapshot = new DirMetaSnapshot(Options.DirectorySeparator);

        var result = await DeserializeSnapshotAsync(stream);

        foreach (var entry in result.Entries!)
        {
            snapshot.AddEntry(entry.ToEntry());
        }

        return snapshot;
    }

    private static async Task<DirMetaSnapshotSchema> DeserializeSnapshotAsync(Stream stream)
    {
        var result = await JsonSerializer.DeserializeAsync<DirMetaSnapshotSchema>(
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
}
