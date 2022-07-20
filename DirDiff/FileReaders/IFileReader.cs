namespace DirDiff.FileReaders;

internal interface IFileReader
{
    Stream Open(string path);
}
