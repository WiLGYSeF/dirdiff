using Wilgysef.DirDiff.DirMetaSnapshotReaders;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using Wilgysef.DirDiff.Tests.Utils;

namespace Wilgysef.DirDiff.Tests.DirMetaSnapshotReadersTests;

public class DirMetaSnapshotTextReaderTest
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

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;

                options.WriteHeader = false;
            });

        var stream = new MemoryStream();
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

            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.HashAlgorithm.ShouldBe(entry.HashAlgorithm);
            resultEntry.CreatedTime.ShouldBe(TruncateToSeconds(entry.CreatedTime!.Value));
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

    [Fact]
    public async Task Read_Hash_LastModifiedTime_FileSize()
    {
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

                options.WriteHeader = false;
            });

        var stream = new MemoryStream();
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

            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

    #region Read Guess

    [Fact]
    public async Task Read_Guess_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
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

                options.WriteHeader = false;
            });

        var stream = new MemoryStream();
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

            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.HashAlgorithm.ShouldBe(entry.HashAlgorithm);
            resultEntry.CreatedTime.ShouldBe(TruncateToSeconds(entry.CreatedTime!.Value));
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

    [Fact]
    public async Task Read_Guess_Hash_CreatedTime_LastModifiedTime_FileSize()
    {
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

                options.WriteHeader = false;
            });

        var stream = new MemoryStream();
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

            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.CreatedTime.ShouldBe(TruncateToSeconds(entry.CreatedTime!.Value));
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

    [Fact]
    public async Task Read_Guess_Hash_LastModifiedTime_FileSize()
    {
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

                options.WriteHeader = false;
            });

        var stream = new MemoryStream();
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

            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

    [Fact]
    public async Task Read_Guess_LastModifiedTime_FileSize()
    {
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

                options.WriteHeader = false;
            });

        var stream = new MemoryStream();
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

            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

    #endregion

    [Fact]
    public async Task Read_Header_Hash_LastModifiedTime_FileSize()
    {
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

                options.WriteHeader = true;
            });

        var stream = new MemoryStream();
        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var reader = new DirMetaSnapshotTextReader();
        var resultSnapshot = await reader.ReadAsync(stream);

        resultSnapshot.Entries.Count.ShouldBe(snapshot.Entries.Count);

        foreach (var entry in snapshot.Entries)
        {
            var resultEntry = resultSnapshot.Entries.Single(e => e.Path == entry.Path);

            resultEntry.Hash.ShouldBe(entry.Hash);
            resultEntry.LastModifiedTime.ShouldBe(TruncateToSeconds(entry.LastModifiedTime!.Value));
            resultEntry.FileSize.ShouldBe(entry.FileSize);
        }
    }

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
