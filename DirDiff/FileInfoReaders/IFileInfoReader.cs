namespace DirDiff.FileInfoReaders;

internal interface IFileInfoReader
{
    Task<FileInfoResult> GetInfoAsync(string path);
}
