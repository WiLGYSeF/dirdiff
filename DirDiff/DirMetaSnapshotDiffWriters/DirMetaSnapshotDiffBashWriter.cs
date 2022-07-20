using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffBashWriter : DirMetaSnapshotDiffWriter
{
    public override DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    protected override string CopyCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject)
    {
        return CopyCommand(reference.Path, subject.Path);
    }

    protected override string CopyCommand(string reference, string subject)
    {
        return $"cp -- {EscapeArgument(reference)} {EscapeArgument(subject)}";
    }

    protected override string MoveCommand(string oldPath, string newPath)
    {
        return $"mv -- {EscapeArgument(oldPath)} {EscapeArgument(newPath)}";
    }

    protected override string TouchCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject)
    {
        return $"touch -d \"$(stat -c %y -- {EscapeArgument(reference.Path)})\" -- {EscapeArgument(subject.Path)}";
    }

    protected override string DeleteCommand(DirMetaSnapshotEntry entry)
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
