using DirDiff.DirMetaSnapshots;
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
        var snapshot = new DirMetaSnapshot();
        var reader = new StreamReader(stream);

        var minColumns = MinimumExpectedColumnCount();

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith('#') || line.Trim().Length == 0)
            {
                continue;
            }

            var entry = TextReaderOptions.ReadGuess
                ? ParseLineGuess(line)
                : ParseLine(line, minColumns);

            snapshot.AddEntry(entry);
        }

        return snapshot;
    }

    private DirMetaSnapshotEntry ParseLineGuess(string line)
    {
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

    private DirMetaSnapshotEntry ParseLine(string line, int minColumns)
    {
        var split = line.Split(TextReaderOptions.Separator);
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

        if (TextReaderOptions.ReadHash)
        {
            hash = split[column] != TextReaderOptions.NoneValue
                ? Convert.FromHexString(split[column])
                : null;
            column++;
        }
        if (TextReaderOptions.ReadHashAlgorithm)
        {
            hashAlgorithm = split[column] != TextReaderOptions.NoneValue
                ? EnumUtils.ParseEnumMemberValue<HashAlgorithm>(split[column])
                : null;
            column++;
        }
        if (TextReaderOptions.ReadCreatedTime)
        {
            createdTime = split[column] != TextReaderOptions.NoneValue
                ? UnixTimeSecondsToDateTime(long.Parse(split[column]))
                : null;
            column++;
        }
        if (TextReaderOptions.ReadLastModifiedTime)
        {
            lastModifiedTime = split[column] != TextReaderOptions.NoneValue
                ? UnixTimeSecondsToDateTime(long.Parse(split[column]))
                : null;
            column++;
        }
        if (TextReaderOptions.ReadFileSize)
        {
            fileSize = split[column] != TextReaderOptions.NoneValue
                ? Convert.ToInt64(split[column])
                : null;
            column++;
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

    private T? GetValue<T>(string value, Func<string, T> convert)
    {
        return value != TextReaderOptions.NoneValue
            ? convert(value)
            : default;
    }

    private static DateTime UnixTimeSecondsToDateTime(long seconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
    }

    private int MinimumExpectedColumnCount()
    {
        if (TextReaderOptions.ReadGuess)
        {
            return 1;
        }

        var bools = new[]
        {
            TextReaderOptions.ReadHash,
            TextReaderOptions.ReadHashAlgorithm,
            TextReaderOptions.ReadCreatedTime,
            TextReaderOptions.ReadLastModifiedTime,
            TextReaderOptions.ReadFileSize,
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
