using DirDiff.Enums;

namespace DirDiff.DirWalkers;

internal class DirWalker : IDirWalker
{
    public DirWalkerOptions Options { get; } = new();

    public DirWalker Configure(Action<DirWalkerOptions> action)
    {
        action(Options);
        return this;
    }

    public IEnumerable<DirWalkerResult> Walk(string path)
    {
        var stack = new Stack<string>();
        stack.Push(Path.GetFullPath(path));

        while (stack.Count > 0)
        {
            var currentPath = stack.Pop();

            if (!Directory.Exists(currentPath))
            {
                if (Options.ThrowIfNotFound)
                {
                    throw new DirectoryNotFoundException(currentPath);
                }
                continue;
            }

            if (Options.ReturnDirectories)
            {
                yield return new DirWalkerResult(currentPath, FileType.Directory);
            }

            foreach (var filename in Directory.EnumerateFiles(currentPath))
            {
                yield return new DirWalkerResult(filename, FileType.File);
            }

            if (Options.KeepDirectoryOrder)
            {
                var directories = Directory.GetDirectories(currentPath);
                for (var i = directories.Length - 1; i >= 0; i--)
                {
                    stack.Push(directories[i]);
                }
            }
            else
            {
                foreach (var dirname in Directory.EnumerateDirectories(currentPath))
                {
                    stack.Push(dirname);
                }
            }
        }
    }
}
