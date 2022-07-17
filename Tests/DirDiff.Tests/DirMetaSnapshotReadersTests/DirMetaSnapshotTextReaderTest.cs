﻿using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Tests.Utils;
using System.Text;

namespace DirDiff.Tests.DirMetaSnapshotReadersTests;

public class DirMetaSnapshotTextReaderTest
{
    [Fact]
    public async Task Read_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader()
            .Configure(options =>
            {
                options.ReadHash = true;
                options.ReadHashAlgorithm = true;
                options.ReadCreatedTime = true;
                options.ReadLastModifiedTime = true;
                options.ReadFileSize = true;
            });
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.FileSize.ShouldBe(entry.FileSize);
            resultEntry.CreatedTime.ShouldBe(TruncateToSeconds(entry.CreatedTime!.Value));
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.HashAlgorithm.ShouldBe(entry.HashAlgorithm);
        }
    }

    [Fact]
    public async Task Read_Hash_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader()
            .Configure(options =>
            {
                options.ReadHash = true;
                options.ReadHashAlgorithm = false;
                options.ReadCreatedTime = false;
                options.ReadLastModifiedTime = true;
                options.ReadFileSize = true;
            });
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.FileSize.ShouldBe(entry.FileSize);
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.Hash.ShouldBe(entry.Hash);
        }
    }

    #region Read Guess

    [Fact]
    public async Task Read_Guess_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader()
            .Configure(options =>
            {
                options.ReadGuess = true;
            });
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.FileSize.ShouldBe(entry.FileSize);
            resultEntry.CreatedTime.ShouldBe(TruncateToSeconds(entry.CreatedTime!.Value));
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.HashAlgorithm.ShouldBe(entry.HashAlgorithm);
        }
    }

    [Fact]
    public async Task Read_Guess_Hash_CreatedTime_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader()
            .Configure(options =>
            {
                options.ReadGuess = true;
            });
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.FileSize.ShouldBe(entry.FileSize);
            resultEntry.CreatedTime.ShouldBe(TruncateToSeconds(entry.CreatedTime!.Value));
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.Hash.ShouldBe(entry.Hash);
        }
    }

    [Fact]
    public async Task Read_Guess_Hash_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader()
            .Configure(options =>
            {
                options.ReadGuess = true;
            });
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.FileSize.ShouldBe(entry.FileSize);
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.Hash.ShouldBe(entry.Hash);
        }
    }

    [Fact]
    public async Task Read_Guess_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = false;
                options.WriteHashAlgorithm = false;
                options.WriteCreatedTime = false;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader()
            .Configure(options =>
            {
                options.ReadGuess = true;
            });
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.FileSize.ShouldBe(entry.FileSize);
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
        }
    }

    #endregion

    private static DateTime TruncateToSeconds(DateTime dateTime)
    {
        return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute,
            dateTime.Second);
    }
}
