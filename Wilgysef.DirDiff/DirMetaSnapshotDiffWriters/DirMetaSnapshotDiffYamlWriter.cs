using Wilgysef.DirDiff.DirMetaSnapshotComparers;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Formats.Asn1;
using System.Text.Json;
using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirMetaSnapshotWriters;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using Wilgysef.DirDiff.Extensions;

namespace Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffYamlWriter : IDirMetaSnapshotDiffWriter
{
    public DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
        var emitter = new Emitter(streamWriter);

        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart());

        emitter.Emit(new Scalar("created"));
        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

        foreach (var entry in diff.CreatedEntries)
        {
            WriteEntry(diff, entry, Options.SecondPrefix, Options, null, emitter);
        }

        emitter.Emit(new SequenceEnd());

        emitter.Emit(new Scalar("deleted"));
        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

        foreach (var entry in diff.DeletedEntries)
        {
            WriteEntry(diff, entry, Options.FirstPrefix, Options, null, emitter);
        }

        emitter.Emit(new SequenceEnd());

        WriteEntryPairArray("modified", diff.ModifiedEntries);
        WriteEntryPairArray("copied", diff.CopiedEntries);
        WriteEntryPairArray("moved", diff.MovedEntries);
        WriteEntryPairArray("touched", diff.TouchedEntries);

        if (Options.WriteUnchanged)
        {
            emitter.Emit(new Scalar("unchanged"));
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (var entry in diff.UnchangedEntries)
            {
                WriteEntry(diff, entry, Options.SecondPrefix, Options, null, emitter);
            }

            emitter.Emit(new SequenceEnd());
        }

        emitter.Emit(new MappingEnd());
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new StreamEnd());

        void WriteEntryPairArray(string propertyName, IReadOnlyCollection<DirMetaSnapshotDiffEntryPair> pairs)
        {
            emitter.Emit(new Scalar(propertyName));
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (var pair in pairs)
            {
                WriteEntryPair(diff, pair, Options, emitter);
            }

            emitter.Emit(new SequenceEnd());
        }
    }

    private static void WriteEntryPair(
        DirMetaSnapshotDiff diff,
        DirMetaSnapshotDiffEntryPair pair,
        DirMetaSnapshotDiffWriterOptions options,
        Emitter emitter)
    {
        emitter.Emit(new MappingStart());

        WriteEntry(diff, pair.First, options.FirstPrefix, options, "first", emitter);
        WriteEntry(diff, pair.Second, options.SecondPrefix, options, "second", emitter);

        emitter.Emit(new MappingEnd());
    }

    private static void WriteEntry(
        DirMetaSnapshotDiff diff,
        DirMetaSnapshotEntry entry,
        string? prefix,
        DirMetaSnapshotDiffWriterOptions options,
        string? propertyName,
        Emitter emitter)
    {
        if (propertyName != null)
        {
            emitter.Emit(new Scalar(propertyName));
        }

        emitter.Emit(new MappingStart());

        var snapshot = diff.GetEntrySnapshot(entry);

        var path = prefix != null
            ? prefix + snapshot.PathWithoutPrefix(entry.Path)
            : entry.Path;
        if (options.DirectorySeparator.HasValue)
        {
            path = snapshot.ChangePathDirectorySeparator(path, options.DirectorySeparator.Value);
        }

        emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Path))));
        emitter.Emit(new Scalar(path));

        if (options.WriteType)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Type))));
            emitter.Emit(new Scalar(entry.Type.ToString()));
        }

        if (entry.Hash != null)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.Hash))));
            emitter.Emit(new Scalar(entry.HashHex!));

            if (entry.HashAlgorithm.HasValue)
            {
                emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.HashAlgorithm))));
                emitter.Emit(new Scalar(entry.HashAlgorithm.Value.ToEnumMemberValue()));
            }
        }

        if (entry.CreatedTime.HasValue)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.CreatedTime))));
            emitter.Emit(new Scalar(entry.CreatedTime.Value.ToString("o")));
        }

        if (entry.LastModifiedTime.HasValue)
        {
            emitter.Emit(new Scalar(ToCamelCase(nameof(DirMetaSnapshotEntrySchema.LastModifiedTime))));
            emitter.Emit(new Scalar(entry.LastModifiedTime.Value.ToString("o")));
        }

        if (entry.FileSize.HasValue)
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
