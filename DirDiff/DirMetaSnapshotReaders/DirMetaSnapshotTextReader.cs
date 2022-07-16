using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;

namespace DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotTextReader : IDirMetaSnapshotReader
{
    private static string SplitString = "  ";

    public DirMetaSnapshotReaderOptions Options => throw new NotImplementedException();

    public DirMetaSnapshotTextReader()
    {

    }

    public async Task<DirMetaSnapshot> Read(Stream stream)
    {
        var snapshot = new DirMetaSnapshot();
        var reader = new StreamReader(stream);

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith('#') || line.Trim().Length == 0)
            {
                continue;
            }

            var entry = ParseLineHashLastModifiedSize(line);

            snapshot.AddEntry(entry);
        }

        return snapshot;
    }

    private DirMetaSnapshotEntry ParseLineGuess(string line)
    {
        var split = line.Split(SplitString);
        var hash = split[0];
        var lastModified = DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(split[1]));
        var fileSize = long.Parse(split[2]);
        var path = split[3..].JoinAsString(SplitString);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            FileSize = fileSize,
            LastModifiedTime = lastModified,
            Hash = Convert.FromHexString(hash),
        };
    }

    private DirMetaSnapshotEntry ParseLineHashLastModifiedSize(string line)
    {
        var split = line.Split(SplitString);
        var hash = split[0];
        var lastModified = DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(split[1]));
        var fileSize = long.Parse(split[2]);
        var path = split[3..].JoinAsString(SplitString);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            FileSize = fileSize,
            LastModifiedTime = lastModified,
            Hash = Convert.FromHexString(hash),
        };
    }

    private DirMetaSnapshotEntry ParseLineHashLastModified(string line)
    {
        var split = line.Split(SplitString);
        var hash = split[0];
        var lastModified = DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(split[1]));
        var path = split[2..].JoinAsString(SplitString);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            LastModifiedTime = lastModified,
            Hash = Convert.FromHexString(hash),
        };
    }

    private DirMetaSnapshotEntry ParseLineHashSize(string line)
    {
        var split = line.Split(SplitString);
        var hash = split[0];
        var fileSize = long.Parse(split[1]);
        var path = split[2..].JoinAsString(SplitString);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            FileSize = fileSize,
            Hash = Convert.FromHexString(hash),
        };
    }

    private DirMetaSnapshotEntry ParseLineHash(string line)
    {
        var split = line.Split(SplitString);
        var hash = split[0];
        var path = split[1..].JoinAsString(SplitString);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            Hash = Convert.FromHexString(hash),
        };
    }

    public IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action)
    {
        throw new NotImplementedException();
    }
}
