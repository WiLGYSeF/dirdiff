using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffBashWriter : IDirMetaSnapshotDiffWriter
{
    public DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(Options);
        return this;
    }

    public async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
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

    private async Task WriteCommand(Stream stream, string command)
    {
        await stream.WriteAsync(Encoding.UTF8.GetBytes(command + Environment.NewLine));
    }

    private string CopyCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject)
    {
        return CopyCommand(reference.Path, subject.Path);
    }

    private string CopyCommand(string reference, string subject)
    {
        return $"cp -- {EscapeArgument(reference)} {EscapeArgument(subject)}";
    }

    private string MoveCommand(DirMetaSnapshotEntry oldEntry, DirMetaSnapshotEntry newEntry)
    {
        return MoveCommand(oldEntry.Path, newEntry.Path);
    }

    private string MoveCommand(string oldPath, string newPath)
    {
        return $"mv -- {EscapeArgument(oldPath)} {EscapeArgument(newPath)}";
    }

    private string TouchCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject)
    {
        return $"touch -d \"$(stat -c %y -- {EscapeArgument(reference.Path)})\" -- {EscapeArgument(subject.Path)}";
    }

    private string DeleteCommand(DirMetaSnapshotEntry entry)
    {
        return $"rm -- {EscapeArgument(entry.Path)}";
    }

    private string EscapeArgument(string argument)
    {
        var builder = new StringBuilder();
        builder.Append('\'');

        for (var i = 0; i < argument.Length; i++)
        {
            switch (argument[i])
            {
                case '\'':
                    builder.Append("'\\''");
                    break;
                default:
                    builder.Append(argument[i]);
                    break;
            }
        }

        builder.Append('\'');
        return builder.ToString();
    }
}
