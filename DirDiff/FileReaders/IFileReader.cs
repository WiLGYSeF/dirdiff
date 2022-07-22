namespace DirDiff.FileReaders;

public interface IFileReader
{
    Stream Open(string path);
}
