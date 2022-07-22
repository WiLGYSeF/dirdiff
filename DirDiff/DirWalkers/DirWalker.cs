using DirDiff.Enums;

namespace DirDiff.DirWalkers;

public class DirWalker : IDirWalker
{
    public DirWalkerOptions Options { get; } = new();

    public IDirWalker Configure(Action<DirWalkerOptions> action)
    {
        action(Options);
        return this;
    }

    public IEnumerable<DirWalkerResult> Walk(string path)
    {
        var fullpath = Path.GetFullPath(path);

        if (File.Exists(fullpath))
        {
            yield return new DirWalkerResult(fullpath, FileType.File);
            yield break;
        }

        var stack = new Stack<(string Path, int Depth)>();
        stack.Push((fullpath, 0));

        while (stack.Count > 0)
        {
            var currentPath = stack.Pop();

            if (!Directory.Exists(currentPath.Path))
            {
                if (Options.ThrowIfNotFound)
                {
                    throw new DirectoryNotFoundException(currentPath.Path);
                }
                continue;
            }

            if (!Options.MinDepthLimit.HasValue || currentPath.Depth >= Options.MinDepthLimit.Value)
            {
                if (Options.ReturnDirectories)
                {
                    yield return new DirWalkerResult(currentPath.Path, FileType.Directory);
                }

                foreach (var filename in Directory.EnumerateFiles(currentPath.Path))
                {
                    yield return new DirWalkerResult(filename, FileType.File);
                }
            }

            if (!Options.MaxDepthLimit.HasValue || currentPath.Depth < Options.MaxDepthLimit.Value)
            {
                if (Options.KeepDirectoryOrder)
                {
                    var directories = Directory.GetDirectories(currentPath.Path);
                    for (var i = directories.Length - 1; i >= 0; i--)
                    {
                        stack.Push((directories[i], currentPath.Depth + 1));
                    }
                }
                else
                {
                    foreach (var dirname in Directory.EnumerateDirectories(currentPath.Path))
                    {
                        stack.Push((dirname, currentPath.Depth + 1));
                    }
                }
            }
        }
    }
}
