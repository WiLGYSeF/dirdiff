using DirDiff.Enums;
using DirDiff.Extensions;

namespace DirDiff.DirMetaSnapshots;

public class DirMetaSnapshotEntry
{
    public string Path { get; }

    public FileType Type { get; }

    public long? FileSize { get; internal set; }

    public DateTime? CreatedTime { get; internal set; }

    public DateTime? LastModifiedTime { get; internal set; }

    public HashAlgorithm? HashAlgorithm { get; internal set; }

    public byte[]? Hash
    {
        get => _hash;
        internal set
        {
            if (Type == FileType.Directory)
            {
                throw new InvalidOperationException();
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

    private byte[]? _hash;
    private string? _hashHex;

    internal DirMetaSnapshotEntry(string path, FileType type)
    {
        Path = path;
        Type = type;
    }
}
