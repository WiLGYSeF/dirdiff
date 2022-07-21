namespace DirDiff.DirWalkers;

public interface IDirWalker
{
    DirWalkerOptions Options { get; }

    IDirWalker Configure(Action<DirWalkerOptions> action);

    IEnumerable<DirWalkerResult> Walk(string path);
}
