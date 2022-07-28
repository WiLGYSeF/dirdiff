using Wilgysef.DirDiff.FileReaders;

namespace Wilgysef.DirDiff.Tests.Utils;

internal class FileReaderMock : IFileReader
{
    public Func<string, Stream> Reader { get; set; }

    public FileReaderMock(Func<string, Stream> reader)
    {
        Reader = reader;
    }

    public Stream Open(string path)
    {
        return Reader(path);
    }
}
