using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;

namespace DirDiff.Tests.Utils;

internal class DirMetaSnapshotEntryBuilder
{
    private const long FileSizeMaxDefault = 1024 * 1024;

    public string? Path { get; set; }

    public FileType Type { get; set; } = FileType.File;

    public long? FileSize
    {
        get => _fileSize;
        set
        {
            _fileSize = value;
            _fileSizeNull = _fileSize == null;
        }
    }

    public DateTime? CreatedTime
    {
        get => _createdTime;
        set
        {
            _createdTime = value;
            _createdTimeNull = _createdTime == null;
        }
    }

    public DateTime? LastModifiedTime
    {
        get => _lastModifiedTime;
        set
        {
            _lastModifiedTime = value;
            _lastModifiedTimeNull = _lastModifiedTime == null;
        }
    }

    public HashAlgorithm? HashAlgorithm
    {
        get => _hashAlgorithm;
        private set
        {
            _hashAlgorithm = value;
            if (_hashAlgorithm == null)
            {
                _hashAlgorithmNull = true;
                _hash = null;
                _hashNull = true;
            }
            else
            {
                _hashAlgorithmNull = false;
            }
        }
    }

    public byte[]? Hash
    {
        get => _hash;
        private set
        {
            _hash = value;
            if (_hash == null)
            {
                _hashNull = true;
                _hashAlgorithm = null;
                _hashAlgorithmNull = true;
            }
            else
            {
                _hashNull = false;
            }
        }
    }

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

    public long? FileSizeMin { get; set; }
    public long? FileSizeMax { get; set; }

    public DirMetaSnapshotEntry Build()
    {
        HashAlgorithm? hashAlgorithm = _hashAlgorithmNull
            ? null
            : _hashAlgorithm ?? (Type != FileType.Directory
                ? Enums.HashAlgorithm.SHA256
                : null);
        byte[]? hash = hashAlgorithm.HasValue && !_hashNull
            ? _hash ?? TestUtils.RandomHash(hashAlgorithm.Value)
            : null;

        return new DirMetaSnapshotEntry(
            Path ?? RandomPath(),
            Type)
        {
            FileSize = _fileSizeNull ? null : _fileSize ?? RandomFileSize(),
            CreatedTime = _createdTimeNull ? null : _createdTime ?? RandomCreatedTime(),
            LastModifiedTime = _lastModifiedTimeNull ? null : _lastModifiedTime ?? RandomLastModifiedTime(),
            HashAlgorithm = hashAlgorithm,
            Hash = hash,
        };
    }

    public DirMetaSnapshotEntryBuilder WithPath(string path)
    {
        Path = path;
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomPath()
    {
        Path = RandomPath();
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomPath(char directorySeparator)
    {
        Path = RandomPath(directorySeparator);
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomPath(string prefix)
    {
        Path = prefix + RandomPath();
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomPath(string prefix, char directorySeparator)
    {
        Path = prefix + RandomPath(directorySeparator);
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithFileType(FileType type)
    {
        Type = type;
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithFileSize(long? fileSize)
    {
        FileSize = fileSize;
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomFileSize()
    {
        FileSize = RandomFileSize();
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithCreatedTime(DateTime? createdTime)
    {
        CreatedTime = createdTime;
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomCreatedTime()
    {
        CreatedTime = RandomCreatedTime();
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithLastModifiedTime(DateTime? lastModifiedTime)
    {
        LastModifiedTime = lastModifiedTime;
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomLastModifiedTime()
    {
        LastModifiedTime = RandomLastModifiedTime();
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithHash(HashAlgorithm algorithm, byte[] hash)
    {
        HashAlgorithm = algorithm;
        Hash = hash;
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithRandomHash(HashAlgorithm? algorithm = null)
    {
        if (!algorithm.HasValue && !HashAlgorithm.HasValue)
        {
            throw new ArgumentNullException(nameof(algorithm));
        }

        HashAlgorithm = algorithm ?? HashAlgorithm!.Value;
        Hash = TestUtils.RandomHash(HashAlgorithm.Value);
        return this;
    }

    public DirMetaSnapshotEntryBuilder WithNoHash()
    {
        HashAlgorithm = null;
        Hash = null;
        return this;
    }

    public DirMetaSnapshotEntryBuilder From(DirMetaSnapshotEntry entry)
    {
        Path = entry.Path;
        Type = entry.Type;
        FileSize = entry.FileSize;
        CreatedTime = entry.CreatedTime;
        LastModifiedTime = entry.LastModifiedTime;
        HashAlgorithm = entry.HashAlgorithm;
        Hash = entry.Hash;
        return this;
    }

    private string RandomPath(char directorySeparator = '/')
    {
        return TestUtils.RandomPath(3, directorySeparator) + TestUtils.RandomExtension();
    }

    private long RandomFileSize()
    {
        return TestUtils.RandomLong(FileSizeMin ?? 0, FileSizeMax ?? FileSizeMaxDefault);
    }

    private DateTime RandomCreatedTime()
    {
        return TestUtils.RandomFileTimestamp();
    }

    private DateTime RandomLastModifiedTime()
    {
        return TestUtils.RandomFileTimestamp();
    }
}
