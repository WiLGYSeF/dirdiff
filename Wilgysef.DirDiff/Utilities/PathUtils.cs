namespace Wilgysef.DirDiff.Utilities;

internal static class PathUtils
{
    public static char GuessDirectorySeparator(IEnumerable<string> paths)
    {
        var forwardSlashCount = 0;
        var backslashCount = 0;

        foreach (var path in paths)
        {
            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    forwardSlashCount++;
                }
                else if (path[i] == '\\')
                {
                    backslashCount++;
                }
            }
        }

        return forwardSlashCount >= backslashCount ? '/' : '\\';
    }
}
