//using DirDiff.DirMetaSnapshots;
//using DirDiff.Enums;
//using DirDiff.Extensions;

//namespace DirDiff.DirMetaSnapshotReaders;

//public class DirMetaSnapshotTextReader : IDirMetaSnapshotReader
//{
//    public DirMetaSnapshotTextReaderOptions TextReaderOptions { get; } = new();

//    public DirMetaSnapshotReaderOptions Options => TextReaderOptions;

//    public DirMetaSnapshotTextReader Configure(Action<DirMetaSnapshotTextReaderOptions> action)
//    {
//        action(TextReaderOptions);
//        return this;
//    }

//    public IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action)
//    {
//        action(TextReaderOptions);
//        return this;
//    }

//    public async Task<DirMetaSnapshot> ReadAsync(Stream stream)
//    {
//        var snapshot = new DirMetaSnapshot();
//        var reader = new StreamReader(stream);

//        string? line;

//        while ((line = await reader.ReadLineAsync()) != null)
//        {
//            if (line.StartsWith('#') || line.Trim().Length == 0)
//            {
//                continue;
//            }

//            var entry = ParseLineHashLastModifiedSize(line);

//            snapshot.AddEntry(entry);
//        }

//        return snapshot;
//    }

//    private DirMetaSnapshotEntry ParseLineGuess(string line)
//    {
//        var split = line.Split(SplitString);
//        var hash = split[0];
//        var lastModified = DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(split[1]));
//        var fileSize = long.Parse(split[2]);
//        var path = split[3..].JoinAsString(SplitString);

//        return new DirMetaSnapshotEntry(path, FileType.File)
//        {
//            FileSize = fileSize,
//            LastModifiedTime = lastModified,
//            Hash = Convert.FromHexString(hash),
//        };
//    }

//    private DirMetaSnapshotEntry ParseLineHashLastModifiedSize(string line)
//    {
//        var split = line.Split(TextReaderOptions.Separator);
//        if (split.Length < )
//            var hash = split[0];
//        var lastModified = DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(split[1]));
//        var fileSize = long.Parse(split[2]);
//        var path = split[3..].JoinAsString(SplitString);

//        return new DirMetaSnapshotEntry(path, FileType.File)
//        {
//            FileSize = fileSize,
//            LastModifiedTime = lastModified,
//            Hash = Convert.FromHexString(hash),
//        };
//    }

//    private int MinimumExpectedColumnCount()
//    {
//        if (TextReaderOptions.ReadGuess)
//        {
//            return 1;
//        }

//        var bools = new[]
//        {
//            TextReaderOptions.ReadHash,
//            TextReaderOptions.ReadHashAlgorithm,
//            TextReaderOptions.ReadCreatedTime,
//            TextReaderOptions.ReadLastModifiedTime,
//            TextReaderOptions.ReadFileSize,
//        };
//        var columnCount = 1;

//        foreach (var b in bools)
//        {
//            if (b)
//            {
//                columnCount++;
//            }
//        }

//        return columnCount;
//    }
//}
