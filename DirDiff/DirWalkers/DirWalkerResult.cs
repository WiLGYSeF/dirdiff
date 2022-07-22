using DirDiff.Enums;

namespace DirDiff.DirWalkers;

public class DirWalkerResult
{
    public string Path { get; }

    public FileType Type { get; }

    public DirWalkerResult(string path, FileType type)
    {
        Path = path;
        Type = type;
    }
}
