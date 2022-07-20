using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public abstract class DirMetaSnapshotDiffWriter : IDirMetaSnapshotDiffWriter
{
    abstract public DirMetaSnapshotDiffWriterOptions Options { get; }

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(Options);
        return this;
    }

    protected abstract string CopyCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject);

    protected abstract string CopyCommand(string reference, string subject);

    protected abstract string MoveCommand(string oldPath, string newPath);

    protected abstract string TouchCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject);

    protected abstract string DeleteCommand(DirMetaSnapshotEntry entry);

    public virtual async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        foreach (var entry in diff.CreatedEntries)
        {
            await WriteCommand(stream, CopyCommand(entry.Path, diff.FirstSnapshot.Prefix + diff.GetEntryPathWithoutPrefix(entry)));
        }

        foreach (var pair in diff.ModifiedEntries)
        {
            await WriteCommand(stream, CopyCommand(pair.Second, pair.First));
        }

        foreach (var pair in diff.CopiedEntries)
        {
            await WriteCommand(stream, CopyCommand(pair.First.Path, diff.FirstSnapshot.Prefix + diff.GetEntryPathWithoutPrefix(pair.Second)));
        }

        foreach (var pair in diff.MovedEntries)
        {
            await WriteCommand(stream, MoveCommand(pair.First.Path, diff.FirstSnapshot.Prefix + diff.GetEntryPathWithoutPrefix(pair.Second)));
        }

        foreach (var pair in diff.TouchedEntries)
        {
            await WriteCommand(stream, TouchCommand(pair.Second, pair.First));
        }

        foreach (var entry in diff.DeletedEntries)
        {
            await WriteCommand(stream, DeleteCommand(entry));
        }
    }

    protected virtual async Task WriteCommand(Stream stream, string command)
    {
        await stream.WriteAsync(Encoding.UTF8.GetBytes(command + Environment.NewLine));
    }
}
