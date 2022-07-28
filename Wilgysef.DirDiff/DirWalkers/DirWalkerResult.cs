using Wilgysef.DirDiff.Enums;

namespace Wilgysef.DirDiff.DirWalkers;

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
