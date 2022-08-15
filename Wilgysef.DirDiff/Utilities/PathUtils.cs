namespace Wilgysef.DirDiff.Utilities;

internal static class PathUtils
{
    /// <summary>
    /// Changes the path directory separator.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="directorySeparator">Original directory separator.</param>
    /// <param name="newDirectorySeparator">New directory separator.</param>
    /// <returns>Path with changed directory separator.</returns>
    public static string ChangePathDirectorySeparator(string path, char directorySeparator, char newDirectorySeparator)
    {
        return string.Join(newDirectorySeparator, GetDirectoryParts(path, directorySeparator));
    }

    /// <summary>
    /// Split path by directory parts.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="directorySeparator">Directory separator.</param>
    /// <returns>Directory parts.</returns>
    public static string[] GetDirectoryParts(string path, char directorySeparator)
    {
        return path.Split(directorySeparator);
    }

    /// <summary>
    /// Guess the path directory separator.
    /// </summary>
    /// <param name="paths">Path.</param>
    /// <returns>Guessed directory separator.</returns>
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
