using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Utilities;

namespace DirDiff.DirMetaSnapshotReaders;

public class DirMetaSnapshotTextReader : IDirMetaSnapshotReader
{
    /// <summary>
    /// Snapshot text reader options.
    /// </summary>
    public DirMetaSnapshotTextReaderOptions TextReaderOptions { get; } = new();

    public DirMetaSnapshotReaderOptions Options => TextReaderOptions;

    /// <summary>
    /// Configure snapshot reader options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
    public DirMetaSnapshotTextReader Configure(Action<DirMetaSnapshotTextReaderOptions> action)
    {
        action(TextReaderOptions);
        return this;
    }

    public IDirMetaSnapshotReader Configure(Action<DirMetaSnapshotReaderOptions> action)
    {
        action(TextReaderOptions);
        return this;
    }

    public async Task<DirMetaSnapshot> ReadAsync(Stream stream)
    {
        var reader = new StreamReader(stream);
        char directorySeparator;

        if (Options.DirectorySeparator.HasValue)
        {
            directorySeparator = Options.DirectorySeparator.Value;
        }
        else
        {
            directorySeparator = PathUtils.GuessDirectorySeparator(GetLines(reader));
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        var minColumns = MinimumExpectedColumnCount(TextReaderOptions);
        var options = TextReaderOptions.Copy();

        string? line;
        var firstLine = true;

        var snapshot = new DirMetaSnapshot(directorySeparator);

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith('#') || line.Trim().Length == 0)
            {
                if (firstLine && line.StartsWith('#'))
                {
                    options.DisableReadOptions();
                    SetReadOptionsFromHeader(options, line);
                    minColumns = MinimumExpectedColumnCount(options);
                }
                firstLine = false;
                continue;
            }
            firstLine = false;

            var entry = TextReaderOptions.ReadGuess
                ? ParseLineGuess(line)
                : ParseLine(line, minColumns, options);

            snapshot.AddEntry(entry);
        }

        return snapshot;
    }

    private DirMetaSnapshotEntry ParseLineGuess(string line)
    {
        // TODO: handle NoneValue

        var split = line.Split(TextReaderOptions.Separator);

        byte[]? hash = null;
        HashAlgorithm? hashAlgorithm = null;
        DateTime? createdTime = null;
        DateTime? lastModifiedTime = null;
        long? fileSize = null;

        var column = 0;

        if (column < split.Length - 1 && IsHexNumeric(split[column]) && split[column].Length >= 16)
        {
            hash = Convert.FromHexString(split[column++]);

            if (column < split.Length - 1 && EnumUtils.TryParseEnumMemberValue<HashAlgorithm>(split[column], out var result))
            {
                hashAlgorithm = result;
                column++;
            }
        }

        if (column < split.Length - 1 && IsNumeric(split[column]))
        {
            if (column + 1 < split.Length - 1 && IsNumeric(split[column + 1]))
            {
                if (column + 2 < split.Length - 1 && IsNumeric(split[column + 2]))
                {
                    createdTime = UnixTimeSecondsToDateTime(long.Parse(split[column]));
                    lastModifiedTime = UnixTimeSecondsToDateTime(long.Parse(split[column + 1]));
                    fileSize = Convert.ToInt64(split[column + 2]);
                    column += 3;
                }
                else
                {
                    lastModifiedTime = UnixTimeSecondsToDateTime(long.Parse(split[column]));
                    fileSize = Convert.ToInt64(split[column + 1]);
                    column += 2;
                }
            }
            else
            {
                fileSize = Convert.ToInt64(split[column]);
                column++;
            }
        }

        var path = split[column..].Join(TextReaderOptions.Separator);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            FileSize = fileSize,
            CreatedTime = createdTime,
            LastModifiedTime = lastModifiedTime,
            Hash = hash,
            HashAlgorithm = hashAlgorithm,
        };
    }

    private DirMetaSnapshotEntry ParseLine(string line, int minColumns, DirMetaSnapshotTextReaderOptions options)
    {
        var split = line.Split(options.Separator);
        if (split.Length < minColumns)
        {
            throw new ArgumentException("Line does not have expected minimum values.", nameof(line));
        }

        byte[]? hash = null;
        HashAlgorithm? hashAlgorithm = null;
        DateTime? createdTime = null;
        DateTime? lastModifiedTime = null;
        long? fileSize = null;

        var column = 0;

        if (options.ReadHash)
        {
            hash = split[column] != options.NoneValue
                ? Convert.FromHexString(split[column])
                : null;
            column++;
        }
        if (options.ReadHashAlgorithm)
        {
            hashAlgorithm = split[column] != options.NoneValue
                ? EnumUtils.ParseEnumMemberValue<HashAlgorithm>(split[column])
                : null;
            column++;
        }
        if (options.ReadCreatedTime)
        {
            createdTime = split[column] != options.NoneValue
                ? UnixTimeSecondsToDateTime(long.Parse(split[column]))
                : null;
            column++;
        }
        if (options.ReadLastModifiedTime)
        {
            lastModifiedTime = split[column] != options.NoneValue
                ? UnixTimeSecondsToDateTime(long.Parse(split[column]))
                : null;
            column++;
        }
        if (options.ReadFileSize)
        {
            fileSize = split[column] != options.NoneValue
                ? Convert.ToInt64(split[column])
                : null;
            column++;
        }

        var path = split[column..].Join(options.Separator);

        return new DirMetaSnapshotEntry(path, FileType.File)
        {
            FileSize = fileSize,
            CreatedTime = createdTime,
            LastModifiedTime = lastModifiedTime,
            Hash = hash,
            HashAlgorithm = hashAlgorithm,
        };
    }

    private void SetReadOptionsFromHeader(DirMetaSnapshotTextReaderOptions options, string header)
    {
        if (!header.StartsWith('#'))
        {
            throw new ArgumentException("Invalid header format.", nameof(header));
        }

        var parts = header[1..].TrimStart().Split(',')
            .Select(p => p.Trim());

        foreach (var part in parts)
        {
            if (part.Equals(DirMetaSnapshotTextWriter.HashHeader, StringComparison.OrdinalIgnoreCase))
            {
                options.ReadHash = true;
            }
            else if (part.Equals(DirMetaSnapshotTextWriter.HashAlgorithmHeader, StringComparison.OrdinalIgnoreCase))
            {
                options.ReadHashAlgorithm = true;
            }
            else if (part.Equals(DirMetaSnapshotTextWriter.CreatedTimeHeader, StringComparison.OrdinalIgnoreCase))
            {
                options.ReadCreatedTime = true;
            }
            else if (part.Equals(DirMetaSnapshotTextWriter.LastModifiedTimeHeader, StringComparison.OrdinalIgnoreCase))
            {
                options.ReadLastModifiedTime = true;
            }
            else if (part.Equals(DirMetaSnapshotTextWriter.FileSizeHeader, StringComparison.OrdinalIgnoreCase))
            {
                options.ReadFileSize = true;
            }
            else if (part.Equals(DirMetaSnapshotTextWriter.PathHeader, StringComparison.OrdinalIgnoreCase))
            {
                // no option to change
            }
            else
            {
                throw new ArgumentException($"Unknown header value: {part}", nameof(header));
            }
        }
    }

    private static IEnumerable<string> GetLines(StreamReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    private static DateTime UnixTimeSecondsToDateTime(long seconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
    }

    private int MinimumExpectedColumnCount(DirMetaSnapshotTextReaderOptions options)
    {
        if (options.ReadGuess)
        {
            return 1;
        }

        var bools = new[]
        {
            options.ReadHash,
            options.ReadHashAlgorithm,
            options.ReadCreatedTime,
            options.ReadLastModifiedTime,
            options.ReadFileSize,
        };
        var columnCount = 1;

        foreach (var b in bools)
        {
            if (b)
            {
                columnCount++;
            }
        }

        return columnCount;
    }

    private static bool IsHexNumeric(string str)
    {
        for (var i = 0; i < str.Length; i++)
        {
            switch (str[i])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    break;
                default:
                    return false;
            }
        }
        return true;
    }

    private static bool IsNumeric(string str)
    {
        for (var i = 0; i < str.Length; i++)
        {
            switch (str[i])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    break;
                default:
                    return false;
            }
        }
        return true;
    }
}
