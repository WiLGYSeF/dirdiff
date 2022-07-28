using Wilgysef.DirDiff.FileInfoReaders;

namespace Wilgysef.DirDiff.Tests.Utils;

internal class FileInfoReaderMock : IFileInfoReader
{
    public Func<string, Task<FileInfoResult>> Reader { get; set; }

    public FileInfoReaderMock(Func<string, Task<FileInfoResult>> reader)
    {
        Reader = reader;
    }

    public async Task<FileInfoResult> GetInfoAsync(string path)
    {
        return await Reader(path);
    }
}
