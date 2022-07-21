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

        var walkerMock = new DirWalkerMock(path =>
        {
            string RandomPath()
            {
                return path.EndsWith(directorySeparator)
                    ? path + TestUtils.RandomPath(3)
                    : path + directorySeparator + TestUtils.RandomPath(3);
            }

            return new[]
            {
                new DirWalkerResult(RandomPath(), FileType.File),
                new DirWalkerResult(RandomPath(), FileType.File),
            };
        });
        var readerMock = new FileReaderMock(path => new MemoryStream(Encoding.UTF8.GetBytes("test")));
        var infoReaderMock = new FileInfoReaderMock(async path =>
        {
            return new FileInfoResult()
            {
                Length = TestUtils.RandomLong(0, 1024 * 1024),
                CreationTimeUtc = TestUtils.RandomFileTimestamp(),
                LastWriteTimeUtc = TestUtils.RandomFileTimestamp(),
            };
        });
        var hasher = new Hasher();

        var builder = new DirMetaSnapshotBuilder(
            walkerMock,
            readerMock,
            infoReaderMock,
            hasher);
        builder.Configure(options =>
        {
            options.DirectorySeparator = directorySeparator;
            options.UseFileSize = true;
            options.UseCreatedTime = true;
            options.UseLastModifiedTime = true;
            options.HashAlgorithm = HashAlgorithm.SHA256;
        });

        builder.AddPath("abc");
        builder.AddPath("def");

        var snapshot = await builder.CreateSnapshotAsync();

        snapshot.Entries.Count.ShouldBe(4);
    }


    [Fact]
    public async Task Update_Snapshot()
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

        foreach (var entry in entries)
        {
            newEntries.Add(new DirMetaSnapshotEntryBuilder().From(entry)
                .Build());
        }

        builder = CreateMockedBuilder(newEntries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);
    }

    private static DirMetaSnapshotBuilder CreateMockedBuilder(IEnumerable<DirMetaSnapshotEntry> entries, char directorySepator)
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
            return entry.Hash!;
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
}
