using System.Text;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.Extensions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Wilgysef.DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotYamlWriter : IDirMetaSnapshotWriter
{
    public DirMetaSnapshotWriterOptions Options { get; } = new();

    public IDirMetaSnapshotWriter Configure(Action<DirMetaSnapshotWriterOptions> action)
    {
        action(Options);
        return this;
    }

    public Task WriteAsync(Stream stream, DirMetaSnapshot snapshot)
    {
        using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
        var emitter = new Emitter(streamWriter);

        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart());

        emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotSchema.DirectorySeparator))));
        emitter.Emit(new Scalar((Options.DirectorySeparator ?? snapshot.DirectorySeparator).ToString()));

        if (Options.WritePrefix)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotSchema.Prefix))));
            emitter.Emit(new Scalar(snapshot.Prefix ?? ""));
        }

        var entries = snapshot.Entries.Where(e => e.Type != FileType.Directory);

        if (Options.SortByPath)
        {
            entries = entries.OrderBy(e => e.Path);
        }

        emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotSchema.Entries))));
        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

        foreach (var entry in entries)
        {
            SerializeEntry(snapshot, entry, emitter);
        }

        emitter.Emit(new SequenceEnd());

        emitter.Emit(new MappingEnd());
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new StreamEnd());

        return Task.CompletedTask;
    }

    private void SerializeEntry(DirMetaSnapshot snapshot, DirMetaSnapshotEntry entry, Emitter emitter)
    {
        emitter.Emit(new MappingStart());

        var path = Options.WritePrefix
            ? entry.Path
            : snapshot.PathWithoutPrefix(entry.Path);

        if (Options.DirectorySeparator.HasValue && Options.DirectorySeparator.Value != snapshot.DirectorySeparator)
        {
            path = snapshot.ChangePathDirectorySeparator(path, Options.DirectorySeparator.Value);
        }

        emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Path))));
        emitter.Emit(new Scalar(path));

        if (Options.WriteType)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Type))));
            emitter.Emit(new Scalar(entry.Type.ToString()));
        }

        if (Options.WriteHash && entry.Hash != null)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Hash))));
            emitter.Emit(new Scalar(entry.HashHex!));

            if (Options.WriteHashAlgorithm && entry.HashAlgorithm.HasValue)
            {
                emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.HashAlgorithm))));
                emitter.Emit(new Scalar(entry.HashAlgorithm.Value.ToEnumMemberValue()));
            }
        }

        if (Options.WriteCreatedTime && entry.CreatedTime.HasValue)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.CreatedTime))));
            emitter.Emit(new Scalar(entry.CreatedTime.Value.ToString("o")));
        }

        if (Options.WriteLastModifiedTime && entry.LastModifiedTime.HasValue)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.LastModifiedTime))));
            emitter.Emit(new Scalar(entry.LastModifiedTime.Value.ToString("o")));
        }

        if (Options.WriteFileSize && entry.FileSize.HasValue)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.FileSize))));
            emitter.Emit(new Scalar(entry.FileSize.Value.ToString()));
        }

        emitter.Emit(new MappingEnd());
    }

    private static string ToCamelCase(string a)
    {
        return a[..1].ToLower() + a[1..];
    }
}
