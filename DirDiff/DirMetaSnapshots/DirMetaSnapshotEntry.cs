using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Hashers;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotEntry
{
    /// <summary>
    /// Entry path.
    /// </summary>
    public string Path { get; internal set; }

    /// <summary>
    /// Entry file type.
    /// </summary>
    public FileType Type { get; }

    /// <summary>
    /// Entry file size.
    /// </summary>
    public long? FileSize { get; internal set; }

    /// <summary>
    /// Entry created time.
    /// </summary>
    public DateTime? CreatedTime { get; internal set; }

    /// <summary>
    /// Entry last modified time.
    /// </summary>
    public DateTime? LastModifiedTime { get; internal set; }

    /// <summary>
    /// Hash algorithm used to create <see cref="Hash"/>.
    /// </summary>
    public HashAlgorithm? HashAlgorithm
    {
        get => _hashAlgorithm;
        internal set
        {
            if (value != null
                && Hash != null
                && Hash.Length != Hasher.GetHashBytes(value.Value))
            {
                throw new InvalidOperationException("Hash byte count does not match expected byte count from hash algorithm.");
            }

            _hashAlgorithm = value;
        }
    }

    /// <summary>
    /// Entry file hash, created using <see cref="HashAlgorithm"/>.
    /// </summary>
    public byte[]? Hash
    {
        get => _hash;
        internal set
        {
            if (Type == FileType.Directory && value != null)
            {
                throw new InvalidOperationException("Cannot set a hash for a file directory.");
            }
            if (value != null
                && HashAlgorithm.HasValue
                && value.Length != Hasher.GetHashBytes(HashAlgorithm.Value))
            {
                throw new InvalidOperationException("Hash byte count does not match expected byte count from hash algorithm.");
            }

            _hash = value;
        }
    }

    /// <summary>
    /// Hexadecimal representation of <see cref="Hash"/>.
    /// </summary>
    public string? HashHex
    {
        get
        {
            if (_hashHex == null && Hash != null)
            {
                _hashHex = Hash.ToHex();
            }

            return _hashHex;
        }
    }

    private HashAlgorithm? _hashAlgorithm;
    private byte[]? _hash;
    private string? _hashHex;

    public DirMetaSnapshotEntry(string path, FileType type)
    {
        Path = path;
        Type = type;
    }

    public bool IsDifferentFrom(
        DirMetaSnapshotEntry entry,
        TimeSpan? timeWindow = null,
        bool ignorePath = false)
    {
        timeWindow ??= TimeSpan.Zero;
        return Type != entry.Type
            || (!ignorePath && Path != entry.Path)
            || NotNullAndDifferent(FileSize, entry.FileSize)
            || NotNullAndDifferent(CreatedTime, entry.CreatedTime, timeWindow.Value)
            || NotNullAndDifferent(LastModifiedTime, entry.LastModifiedTime, timeWindow.Value)
            || NotNullAndDifferent(HashAlgorithm, entry.HashAlgorithm)
            || (Hash != null && entry.Hash != null && !Hash.SequenceEqual(entry.Hash));
    }

    public void CopyKnownPropertiesFrom(DirMetaSnapshotEntry entry)
    {
        if (!FileSize.HasValue && entry.FileSize.HasValue)
        {
            FileSize = entry.FileSize;
        }
        if (!CreatedTime.HasValue && entry.CreatedTime.HasValue)
        {
            CreatedTime = entry.CreatedTime;
        }
        if (!LastModifiedTime.HasValue && entry.LastModifiedTime.HasValue)
        {
            LastModifiedTime = entry.LastModifiedTime;
        }
        if (!HashAlgorithm.HasValue && entry.HashAlgorithm.HasValue)
        {
            HashAlgorithm = entry.HashAlgorithm;
        }
        if (Hash == null && entry.Hash != null)
        {
            Hash = entry.Hash;
        }
    }

    /// <summary>
    /// Copies the entry.
    /// </summary>
    /// <returns>Copied entry.</returns>
    public DirMetaSnapshotEntry Copy()
    {
        return new DirMetaSnapshotEntry(Path, Type)
        {
            FileSize = FileSize,
            CreatedTime = CreatedTime,
            LastModifiedTime = LastModifiedTime,
            HashAlgorithm = HashAlgorithm,
            Hash = Hash,
        };
    }

    private static bool NotNullAndDifferent<T>(T? a, T? b) where T : struct
    {
        return a.HasValue && b.HasValue && !a.Value.Equals(b.Value);
    }

    private static bool NotNullAndDifferent(DateTime? a, DateTime? b, TimeSpan window)
    {
        return a.HasValue && b.HasValue && !a.Value.Within(b.Value, window);
    }
}
