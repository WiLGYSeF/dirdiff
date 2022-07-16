using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotTextWriter : IDirMetaSnapshotWriter
{
    public DirMetaSnapshotWriterOptions Options { get; } = new();

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshot snapshot)
    {
        var builder = new StringBuilder();

        foreach (var entry in snapshot.Entries)
        {
            if (entry.Type == FileType.Directory)
            {
                continue;
            }

            builder.Clear();

            if (Options.WriteHash)
            {
                builder.Append(entry.Hash != null ? entry.HashHex : Options.NoneValue);
                builder.Append(Options.Separator);

                if (Options.WriteHashAlgorithm)
                {
                    builder.Append(entry.HashAlgorithm.HasValue ? entry.HashAlgorithm.ToEnumMemberValue() : Options.NoneValue);
                    builder.Append(Options.Separator);
                }
            }

            if (Options.WriteCreatedTime)
            {
                builder.Append(entry.CreatedTime.HasValue
                    ? Math.Floor(entry.CreatedTime.Value.ToUnixTimestamp()).ToString()
                    : Options.NoneValue);
                builder.Append(Options.Separator);
            }

            if (Options.WriteLastModifiedTime)
            {
                builder.Append(entry.LastModifiedTime.HasValue
                    ? Math.Floor(entry.LastModifiedTime.Value.ToUnixTimestamp()).ToString()
                    : Options.NoneValue);
                builder.Append(Options.Separator);
            }

            if (Options.WriteFileSize)
            {
                builder.Append(entry.FileSize != null ? entry.FileSize.ToString() : Options.NoneValue);
                builder.Append(Options.Separator);
            }

            builder.AppendLine(entry.Path);

            await stream.WriteAsync(Encoding.UTF8.GetBytes(builder.ToString()));
        }
    }
}
