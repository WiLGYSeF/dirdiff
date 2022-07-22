namespace DirDiff.FileInfoReaders;

public class FileInfoReader : IFileInfoReader
{
    public Task<FileInfoResult> GetInfoAsync(string path)
    {
        var info = new FileInfo(path);

        return Task.FromResult(new FileInfoResult
        {
            Length = info.Length,
            CreationTimeUtc = info.CreationTimeUtc,
            LastWriteTimeUtc = info.LastWriteTimeUtc,
        });
    }
}
