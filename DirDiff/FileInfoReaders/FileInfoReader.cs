namespace DirDiff.FileInfoReaders;

public class FileInfoReader : IFileInfoReader
{
    public async Task<FileInfoResult> GetInfoAsync(string path)
    {
        var info = new FileInfo(path);

        return new FileInfoResult
        {
            Length = info.Length,
            CreationTimeUtc = info.CreationTimeUtc,
            LastWriteTimeUtc = info.LastWriteTimeUtc,
        };
    }
}
