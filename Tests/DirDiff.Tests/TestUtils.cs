using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using System.Text;

namespace DirDiff.Tests;

internal static class TestUtils
{
    private static readonly Random _random = new Random();

    public class DirMetaSnapshotEntryBuilder
    {
        private const long FileSizeMaxDefault = 1024 * 1024;

        public long? FileSizeMin { get; set; }
        public long? FileSizeMax { get; set; }

        private string? _path;

        private FileType? _type;

        private long? _fileSize;
        private bool _fileSizeNull;

        private DateTime? _createdTime;
        private bool _createdTimeNull;

        private DateTime? _lastModifiedTime;
        private bool _lastModifiedTimeNull;

        private HashAlgorithm? _hashAlgorithm;
        private bool _hashAlgorithmNull;

        private byte[]? _hash;
        private bool _hashNull;

        public DirMetaSnapshotEntry Build()
        {
            return new DirMetaSnapshotEntry(
                _path ?? RandomPath(3) + RandomExtension(),
                _type ?? FileType.File)
            {
                FileSize = _fileSizeNull ? null : _fileSize ?? RandomLong(FileSizeMin ?? 0, FileSizeMax ?? FileSizeMaxDefault),
                CreatedTime = _createdTimeNull ? null : _createdTime ?? RandomFileTimestamp(),
                LastModifiedTime = _lastModifiedTimeNull ? null : _lastModifiedTime ?? RandomFileTimestamp(),
                HashAlgorithm = _hashAlgorithmNull ? null : _hashAlgorithm ?? HashAlgorithm.SHA256,
                Hash = _hashNull ? null : _hash ?? RandomBytes(32),
            };
        }

        public DirMetaSnapshotEntryBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public DirMetaSnapshotEntryBuilder WithFileType(FileType type)
        {
            _type = type;
            return this;
        }

        public DirMetaSnapshotEntryBuilder WithFileSize(long? fileSize)
        {
            _fileSize = fileSize;
            _fileSizeNull = fileSize == null;
            return this;
        }

        public DirMetaSnapshotEntryBuilder WithCreatedTime(DateTime? createdTime)
        {
            _createdTime = createdTime;
            _createdTimeNull = createdTime == null;
            return this;
        }

        public DirMetaSnapshotEntryBuilder WithLastModifiedTime(DateTime? lastModifiedTime)
        {
            _lastModifiedTime = lastModifiedTime;
            _lastModifiedTimeNull = lastModifiedTime == null;
            return this;
        }

        public DirMetaSnapshotEntryBuilder WithHash(HashAlgorithm algorithm, byte[] hash)
        {
            _hashAlgorithm = algorithm;
            _hashAlgorithmNull = false;
            _hash = hash;
            _hashNull = false;
            return this;
        }

        public DirMetaSnapshotEntryBuilder WithNoHash()
        {
            _hashAlgorithm = null;
            _hashAlgorithmNull = true;
            _hash = null;
            _hashNull = true;
            return this;
        }

        public static DirMetaSnapshotEntry From(DirMetaSnapshotEntry entry)
        {
            return new DirMetaSnapshotEntry(entry.Path, entry.Type)
            {
                FileSize = entry.FileSize,
                CreatedTime = entry.CreatedTime,
                LastModifiedTime = entry.LastModifiedTime,
                HashAlgorithm = entry.HashAlgorithm,
                Hash = entry.Hash,
            };
        }
    }

    #region Random

    public static int RandomInt()
    {
        return _random.Next();
    }

    public static int RandomInt(int max)
    {
        return RandomInt(0, max);
    }

    public static int RandomInt(int min, int max)
    {
        return _random.Next(min, max);
    }

    public static long RandomLong()
    {
        return _random.NextInt64();
    }

    public static long RandomLong(long max)
    {
        return _random.NextInt64(0, max);
    }

    public static long RandomLong(long min, long max)
    {
        return _random.NextInt64(min, max);
    }

    public static double RandomDouble()
    {
        return _random.NextDouble();
    }

    public static double RandomDouble(double max)
    {
        return RandomDouble(0, max);
    }

    public static double RandomDouble(double min, double max)
    {
        return RandomDouble() * (max - min) + min;
    }

    public static byte[] RandomBytes(int length)
    {
        byte[] bytes = new byte[length];
        _random.NextBytes(bytes);
        return bytes;
    }

    public static string RandomString(int length)
    {
        var charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        return RandomString(length, charset);
    }

    public static string RandomString(int length, char[] charset)
    {
        var sbuilder = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            sbuilder.Append(charset[RandomInt(charset.Length)]);
        }
        return sbuilder.ToString();
    }

    public static string RandomPath(int parts, string separator = "/")
    {
        var minPartLength = 4;
        var maxPartLength = 24;

        var sbuilder = new StringBuilder();
        for (var i = 0; i < parts; i++)
        {
            sbuilder.Append(RandomString(RandomInt(minPartLength, maxPartLength)));
            if (i < parts - 1)
            {
                sbuilder.Append(separator);
            }
        }
        return sbuilder.ToString();
    }

    public static string RandomExtension()
    {
        return "." + RandomString(3);
    }

    public static DateTime RandomFileTimestamp()
    {
        return RandomDateTime(new DateTime(2000, 1, 1), new DateTime(2100, 1, 1), DateTimePrecision.Milliseconds);
    }

    public static DateTime RandomDateTime(DateTime start, DateTime end, DateTimePrecision precision)
    {
        return precision switch
        {
            DateTimePrecision.Milliseconds => start + TimeSpan.FromMilliseconds(RandomDouble((end - start).TotalMilliseconds)),
            DateTimePrecision.Seconds => start + TimeSpan.FromSeconds(RandomDouble((end - start).TotalSeconds)),
            DateTimePrecision.Minutes => start + TimeSpan.FromMinutes(RandomDouble((end - start).TotalMinutes)),
            DateTimePrecision.Hours => start + TimeSpan.FromHours(RandomDouble((end - start).TotalHours)),
            DateTimePrecision.Days => start + TimeSpan.FromDays(RandomDouble((end - start).TotalDays)),
            _ => throw new NotImplementedException(),
        };
    }

    public enum DateTimePrecision
    {
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days,
    }

    #endregion
}
