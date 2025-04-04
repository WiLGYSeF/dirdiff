using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.Utilities;

namespace Wilgysef.DirDiff.DirMetaSnapshotWriters;

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

        var type = Type != null
            ? EnumUtils.Parse<FileType>(Type)
            : FileType.File;

        var entry = new DirMetaSnapshotEntry(Path, type)
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
