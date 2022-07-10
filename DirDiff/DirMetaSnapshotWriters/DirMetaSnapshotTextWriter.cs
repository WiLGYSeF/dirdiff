using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotTextWriter : IDirMetaSnapshotWriter
{
    public async Task Write(Stream stream, DirMetaSnapshot snapshot)
    {
        var builder = new StringBuilder();

        foreach (var entry in snapshot.Entries)
        {
            if (entry.Type == FileType.Directory)
            {
                continue;
            }

            builder.Clear();

            builder.Append(entry.Hash != null ? entry.HashHex : "-");
            builder.Append("  ");

            builder.Append(entry.LastModifiedTime.HasValue
                ? Math.Floor(entry.LastModifiedTime.Value.ToUnixTimestamp()).ToString()
                : "-");
            builder.Append("  ");

            builder.Append(entry.FileSize != null ? entry.FileSize.ToString() : "-");
            builder.Append("  ");

            builder.AppendLine(entry.Path);

            await stream.WriteAsync(Encoding.UTF8.GetBytes(builder.ToString()));
        }
    }
}
