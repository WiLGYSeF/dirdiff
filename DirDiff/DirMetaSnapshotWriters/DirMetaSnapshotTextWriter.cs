using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotTextWriter : IDirMetaSnapshotWriter
{
    public DirMetaSnapshotTextWriterOptions TextWriterOptions { get; } = new();

    public DirMetaSnapshotWriterOptions Options => TextWriterOptions;

    public DirMetaSnapshotTextWriter Configure(Action<DirMetaSnapshotTextWriterOptions> action)
    {
        action(TextWriterOptions);
        return this;
    }

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action)
    {
        action(TextWriterOptions);
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
                builder.Append(entry.Hash != null ? entry.HashHex : TextWriterOptions.NoneValue);
                builder.Append(TextWriterOptions.Separator);

                if (Options.WriteHashAlgorithm)
                {
                    builder.Append(entry.HashAlgorithm.HasValue ? entry.HashAlgorithm.ToEnumMemberValue() : TextWriterOptions.NoneValue);
                    builder.Append(TextWriterOptions.Separator);
                }
            }

            if (Options.WriteCreatedTime)
            {
                builder.Append(entry.CreatedTime.HasValue
                    ? ((DateTimeOffset)entry.CreatedTime.Value).ToUnixTimeSeconds()
                    : TextWriterOptions.NoneValue);
                builder.Append(TextWriterOptions.Separator);
            }

            if (Options.WriteLastModifiedTime)
            {
                builder.Append(entry.LastModifiedTime.HasValue
                    ? ((DateTimeOffset)entry.LastModifiedTime.Value).ToUnixTimeSeconds()
                    : TextWriterOptions.NoneValue);
                builder.Append(TextWriterOptions.Separator);
            }

            if (Options.WriteFileSize)
            {
                builder.Append(entry.FileSize != null ? entry.FileSize.ToString() : TextWriterOptions.NoneValue);
                builder.Append(TextWriterOptions.Separator);
            }

            builder.AppendLine(entry.Path);

            await stream.WriteAsync(Encoding.UTF8.GetBytes(builder.ToString()));
        }
    }
}
