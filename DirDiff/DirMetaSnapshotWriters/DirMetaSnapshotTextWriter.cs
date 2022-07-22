using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Extensions;
using System.Text;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotTextWriter : IDirMetaSnapshotWriter
{
    public static readonly string HashHeader = "Hash";
    public static readonly string HashAlgorithmHeader = "HashAlgorithm";
    public static readonly string CreatedTimeHeader = "CreatedTime";
    public static readonly string LastModifiedTimeHeader = "LastModifiedTime";
    public static readonly string FileSizeHeader = "FileSize";
    public static readonly string PathHeader = "Path";

    /// <summary>
    /// Snapshot text writer options.
    /// </summary>
    public DirMetaSnapshotTextWriterOptions TextWriterOptions { get; } = new();

    public DirMetaSnapshotWriterOptions Options => TextWriterOptions;

    /// <summary>
    /// Configures snapshot writer options.
    /// </summary>
    /// <param name="action">Configure action.</param>
    /// <returns></returns>
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

        if (TextWriterOptions.WriteHeader)
        {
            AppendHeaderLine(builder);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(builder.ToString()));
        }

        foreach (var entry in snapshot.Entries)
        {
            if (entry.Type == FileType.Directory)
            {
                continue;
            }

            builder.Clear();
            AppendEntryLine(builder, snapshot, entry);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(builder.ToString()));
        }
    }

    private void AppendHeaderLine(StringBuilder builder)
    {
        var values = new List<string>();

        if (Options.WriteHash)
        {
            values.Add(HashHeader);

            if (Options.WriteHashAlgorithm)
            {
                values.Add(HashAlgorithmHeader);
            }
        }
        if (Options.WriteCreatedTime)
        {
            values.Add(CreatedTimeHeader);
        }
        if (Options.WriteLastModifiedTime)
        {
            values.Add(LastModifiedTimeHeader);
        }
        if (Options.WriteFileSize)
        {
            values.Add(FileSizeHeader);
        }

        values.Add(PathHeader);

        builder.Append("# ");

        for (var i = 0; i < values.Count; i++)
        {
            builder.Append(values[i]);
            if (i < values.Count - 1)
            {
                builder.Append(',');
            }
        }
        builder.AppendLine();
    }

    private void AppendEntryLine(StringBuilder builder, DirMetaSnapshot snapshot, DirMetaSnapshotEntry entry)
    {
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

        builder.AppendLine(Options.WritePrefix ? entry.Path : snapshot.PathWithoutPrefix(entry.Path));
    }
}
