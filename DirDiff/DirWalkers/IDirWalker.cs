namespace DirDiff.DirWalkers;

internal interface IDirWalker
{
    DirWalkerOptions Options { get; }

    IDirWalker Configure(Action<DirWalkerOptions> action);

    IEnumerable<DirWalkerResult> Walk(string path);
}
