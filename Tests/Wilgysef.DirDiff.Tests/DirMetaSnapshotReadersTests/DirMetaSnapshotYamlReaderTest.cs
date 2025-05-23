﻿using Wilgysef.DirDiff.DirMetaSnapshotReaders;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using Wilgysef.DirDiff.Tests.Utils;

namespace Wilgysef.DirDiff.Tests.DirMetaSnapshotReadersTests;

public class DirMetaSnapshotYamlReaderTest
{
    [Fact]
    public async Task Read_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotYamlWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotYamlReader();
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.ShouldBeEquivalentTo(snapshot.Entries);
    }
}
