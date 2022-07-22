using DirDiff.DirMetaSnapshots;
using DirDiff.DirWalkers;
using DirDiff.Enums;
using DirDiff.FileInfoReaders;
using DirDiff.Hashers;
using DirDiff.Tests.Utils;
using System.Text;

namespace DirDiff.Tests.DirMetaSnapshotTests;

public class DirMetaSnapshotBuilderTest
{
    [Fact]
    public async Task Create_Snapshot()
    {
        var directorySeparator = '/';

        var entries = new List<DirMetaSnapshotEntry>();

        entries.Add(new DirMetaSnapshotEntryBuilder().Build());
        entries.Add(new DirMetaSnapshotEntryBuilder().Build());
        entries.Add(new DirMetaSnapshotEntryBuilder().Build());
        entries.Add(new DirMetaSnapshotEntryBuilder().Build());

        var builder = CreateMockedBuilder(entries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        snapshot.Entries.Count.ShouldBe(4);
    }

    [Fact]
    public async Task Update_Snapshot_Created_Deleted_Updated()
    {
        var directorySeparator = '/';

        var entries = new List<DirMetaSnapshotEntry>();

        entries.Add(new DirMetaSnapshotEntryBuilder().Build());
        entries.Add(new DirMetaSnapshotEntryBuilder().Build());
        entries.Add(new DirMetaSnapshotEntryBuilder().Build());
        entries.Add(new DirMetaSnapshotEntryBuilder().Build());

        var builder = CreateMockedBuilder(entries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntries = new List<DirMetaSnapshotEntry>();

        foreach (var entry in entries.Take(2))
        {
            newEntries.Add(new DirMetaSnapshotEntryBuilder().From(entry)
                .Build());
        }

        var newAddedEntry = new DirMetaSnapshotEntryBuilder().Build();
        newEntries.Add(newAddedEntry);

        builder = CreateMockedBuilder(newEntries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in newEntries)
        {
            builder.AddPath(entry.Path);
        }

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Entries.Count.ShouldBe(3);
        
        foreach (var expected in entries.Take(2))
        {
            ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == expected.Path), expected);
        }

        ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == newAddedEntry.Path), newAddedEntry);
    }

    [Fact]
    public async Task Update_Snapshot_Updated_Contents()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithRandomLastModifiedTime()
            .WithRandomFileSize()
            .WithRandomHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Entries.Count.ShouldBe(1);

        ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == newEntry.Path), newEntry);
    }

    [Fact]
    public async Task Update_Snapshot_Old_LastModified_Null()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder()
            .WithLastModifiedTime(null)
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithRandomLastModifiedTime()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.LastModifiedTime.ShouldBe(newEntry.LastModifiedTime);
    }

    [Fact]
    public async Task Update_Snapshot_New_LastModified_Null()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithLastModifiedTime(null)
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.LastModifiedTime.ShouldBe(entry.LastModifiedTime);
    }

    #region Hash Update Tests

    [Fact]
    public async Task Update_Snapshot_Old_Hash_Null()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder()
            .WithNoHash()
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithRandomHash(HashAlgorithm.SHA256)
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeEquivalentTo(newEntry.Hash);
    }

    [Fact]
    public async Task Update_Snapshot_New_Hash_Null()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeEquivalentTo(entry.Hash);
    }

    [Fact]
    public async Task Update_Snapshot_Old_New_Hash_Null()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder()
            .WithNoHash()
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator, useRandomHash: true)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_Snapshot_New_Hash_Null_No_Algorithm()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = null;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeEquivalentTo(entry.Hash);
    }

    [Fact]
    public async Task Update_Snapshot_Old_New_Hash_Null_No_Algorithm()
    {
        var directorySeparator = '/';

        var entry = new DirMetaSnapshotEntryBuilder()
            .WithNoHash()
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = new DirMetaSnapshotEntryBuilder().From(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = null;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeNull();
    }

    #endregion

    private static DirMetaSnapshotBuilder CreateMockedBuilder(
        IEnumerable<DirMetaSnapshotEntry> entries,
        char directorySepator,
        bool useRandomHash = false)
    {
        var entryMap = entries.ToDictionary(e => e.Path);

        var walkerMock = new DirWalkerMock(path =>
        {
            var prefix = path.EndsWith(directorySepator) ? path : path + directorySepator;
            return entries
                .Where(e => e.Path.StartsWith(prefix) || e.Path == path)
                .Select(e => new DirWalkerResult(e.Path, e.Type));
        });
        var readerMock = new FileReaderMock(path =>
        {
            if (!entryMap.TryGetValue(path, out var entry))
            {
                throw new NotImplementedException();
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(entry.Path));
        });
        var infoReaderMock = new FileInfoReaderMock(async path =>
        {
            if (!entryMap.TryGetValue(path, out var entry))
            {
                throw new NotImplementedException();
            }
            return new FileInfoResult()
            {
                Length = entry.FileSize,
                CreationTimeUtc = entry.CreatedTime,
                LastWriteTimeUtc = entry.LastModifiedTime,
            };
        });
        var hasherMock = new HasherMock((algorithm, stream) =>
        {
            using var reader = new StreamReader(stream);
            var path = reader.ReadToEnd();
            if (!entryMap.TryGetValue(path, out var entry))
            {
                throw new NotImplementedException();
            }
            return useRandomHash
                ? TestUtils.RandomBytes(Hasher.GetHashBytes(HashAlgorithm.SHA256))
                : entry.Hash!;
        });

        return new DirMetaSnapshotBuilder(
            walkerMock,
            readerMock,
            infoReaderMock,
            hasherMock).Configure(options =>
            {
                options.DirectorySeparator = directorySepator;
            });
    }

    private void ShouldBeEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry expected)
    {
        entry.Path.ShouldBe(expected.Path);
    }
}
