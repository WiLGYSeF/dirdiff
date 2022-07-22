using DirDiff.DirWalkers;

namespace DirDiff.Tests.Utils;

internal class DirWalkerMock : IDirWalker
{
    public DirWalkerOptions Options { get; } = new();

    public Func<string, IEnumerable<DirWalkerResult>> Walker { get; set; }

    public DirWalkerMock(Func<string, IEnumerable<DirWalkerResult>> walker)
    {
        Walker = walker;
    }

    public IDirWalker Configure(Action<DirWalkerOptions> action)
    {
        action(Options);
        return this;
    }

    public IEnumerable<DirWalkerResult> Walk(string path)
    {
        return Walker(path);
    }
}
