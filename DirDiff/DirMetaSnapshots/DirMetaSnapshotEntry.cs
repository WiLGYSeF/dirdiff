using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Hashers;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotEntry
{
    /// <summary>
    /// Entry path.
    /// </summary>
    public string Path { get; }

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

    internal DirMetaSnapshotEntry(string path, FileType type)
    {
        Path = path;
        Type = type;
    }
}
