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
}
