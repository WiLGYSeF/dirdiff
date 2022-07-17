using DirDiff.Enums;
using DirDiff.Extensions;
using DirDiff.Hashers;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotEntry
{
    public string Path { get; }

    public FileType Type { get; }

    public long? FileSize { get; internal set; }

    public DateTime? CreatedTime { get; internal set; }

    public DateTime? LastModifiedTime { get; internal set; }

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
