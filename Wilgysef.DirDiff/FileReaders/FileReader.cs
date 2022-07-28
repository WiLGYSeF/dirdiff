namespace Wilgysef.DirDiff.FileReaders;

public class FileReader : IFileReader
{
    public Stream Open(string path)
    {
        return File.OpenRead(path);
    }
}
