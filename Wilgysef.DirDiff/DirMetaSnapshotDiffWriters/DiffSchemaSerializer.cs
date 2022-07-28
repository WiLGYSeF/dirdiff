using Wilgysef.DirDiff.DirMetaSnapshotComparers;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.Extensions;

namespace Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;

internal static class DiffSchemaSerializer
{
    public static object SerializeDiff(DirMetaSnapshotDiff diff, DirMetaSnapshotDiffWriterOptions options)
    {
        return new
        {
            Created = diff.CreatedEntries.Select(e => SerializeEntry(diff, e, options.SecondPrefix, options)),
            Deleted = diff.DeletedEntries.Select(e => SerializeEntry(diff, e, options.FirstPrefix, options)),
            Modified = diff.ModifiedEntries.Select(p => SerializeEntryPair(diff, p, options)),
            Copied = diff.CopiedEntries.Select(p => SerializeEntryPair(diff, p, options)),
            Moved = diff.MovedEntries.Select(p => SerializeEntryPair(diff, p, options)),
            Touched = diff.TouchedEntries.Select(p => SerializeEntryPair(diff, p, options)),
            Unchanged = diff.UnchangedEntries.Select(e => SerializeEntry(diff, e, options.SecondPrefix, options)),
        };
    }

    public static object SerializeEntryPair(
        DirMetaSnapshotDiff diff,
        DirMetaSnapshotDiffEntryPair pair,
        DirMetaSnapshotDiffWriterOptions options)
    {
        return new
        {
            First = SerializeEntry(diff, pair.First, options.FirstPrefix, options),
            Second = SerializeEntry(diff, pair.Second, options.SecondPrefix, options),
        };
    }

    public static Dictionary<string, object> SerializeEntry(
        DirMetaSnapshotDiff diff,
        DirMetaSnapshotEntry entry,
        string? prefix,
        DirMetaSnapshotDiffWriterOptions options)
    {
        var snapshot = diff.GetEntrySnapshot(entry);

        var path = prefix != null
            ? prefix + snapshot.PathWithoutPrefix(entry.Path)
            : entry.Path;
        if (options.DirectorySeparator.HasValue)
        {
            path = snapshot.ChangePathDirectorySeparator(path, options.DirectorySeparator.Value);
        }

        var dictionary = new Dictionary<string, object>
        {
            { "path", path },
            { "type", entry.Type },
        };

        if (entry.Hash != null)
        {
            dictionary["hash"] = entry.HashHex!;

            if (entry.HashAlgorithm.HasValue)
            {
                dictionary["hashAlgorithm"] = entry.HashAlgorithm.Value.ToEnumMemberValue();
            }
        }

        if (entry.CreatedTime.HasValue)
        {
            dictionary["createdTime"] = entry.CreatedTime.Value;
        }

        if (entry.LastModifiedTime.HasValue)
        {
            dictionary["lastModifiedTime"] = entry.LastModifiedTime.Value;
        }

        if (entry.FileSize.HasValue)
        {
            dictionary["fileSize"] = entry.FileSize.Value;
        }

        return dictionary;
    }
}
