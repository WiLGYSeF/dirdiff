namespace Wilgysef.DirDiff.FileReaders;

public interface IFileReader
{
    Stream Open(string path);
}
