using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;

namespace DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotTextReader
{
    private static string SplitString = "  ";

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

            var split = line.Split(SplitString);
            var hash = split[0];
            var lastModified = DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(split[1]));
            var fileSize = long.Parse(split[2]);
            var path = split[3..].JoinAsString(SplitString);

            var entry = new DirMetaSnapshotEntry(path, FileType.File)
            {
                FileSize = fileSize,
                LastModifiedTime = lastModified,
                Hash = Convert.FromHexString(hash),
            };

            snapshot.AddEntry(entry);
        }

        return snapshot;
    }
}
