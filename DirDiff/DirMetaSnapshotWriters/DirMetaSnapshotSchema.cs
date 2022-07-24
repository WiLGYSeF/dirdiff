using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Utilities;

namespace DirDiff.DirMetaSnapshotWriters;

internal class DirMetaSnapshotSchema
{
    public char? DirectorySeparator { get; set; }

    public string? Prefix { get; set; }

    public ICollection<DirMetaSnapshotEntrySchema>? Entries { get; set; }
}

internal class DirMetaSnapshotEntrySchema
{
    public string? Path { get; set; }

    public string? Type { get; set; }

    public long? FileSize { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? LastModifiedTime { get; set; }

    public string? HashAlgorithm { get; set; }

    public string? Hash { get; set; }

    public DirMetaSnapshotEntry ToEntry()
    {
        if (Path == null)
        {
            throw new InvalidOperationException($"Entry does not have \"{nameof(Path)}\" value.");
        }
        if (Type == null)
        {
            throw new InvalidOperationException($"Entry does not have \"{nameof(Type)}\" value.");
        }

        var entry = new DirMetaSnapshotEntry(Path, EnumUtils.Parse<FileType>(Type))
        {
            FileSize = FileSize,
            CreatedTime = CreatedTime,
            LastModifiedTime = LastModifiedTime,
            HashAlgorithm = HashAlgorithm != null ? EnumUtils.ParseEnumMemberValue<HashAlgorithm>(HashAlgorithm) : null,
            Hash = Hash != null ? Convert.FromHexString(Hash) : null,
        };

        return entry;
    }
}
