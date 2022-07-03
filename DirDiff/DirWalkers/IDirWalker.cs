namespace DirDiff.DirWalkers;

internal interface IDirWalker
{
    DirWalkerOptions Options { get; }

    DirWalker Configure(Action<DirWalkerOptions> action);

    IEnumerable<DirWalkerResult> Walk(string path);
}
