namespace DirDiff.FileReaders;

internal class FileReader : IFileReader
{
    public Stream Open(string path)
    {
        return File.OpenRead(path);
    }
}
