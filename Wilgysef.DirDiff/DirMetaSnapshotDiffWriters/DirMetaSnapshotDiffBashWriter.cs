using Wilgysef.DirDiff.DirMetaSnapshots;
using System.Text;

namespace Wilgysef.DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffBashWriter : DirMetaSnapshotDiffCommandWriter
{
    public override DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    protected override string CopyCommand(string reference, string subject)
    {
        return $"cp -- {EscapeArgument(reference)} {EscapeArgument(subject)}";
    }

    protected override string MoveCommand(string oldPath, string newPath)
    {
        return $"mv -- {EscapeArgument(oldPath)} {EscapeArgument(newPath)}";
    }

    protected override string TouchCommand(string reference, string subject)
    {
        return $"touch -d \"$(stat -c %y -- {EscapeArgument(reference)})\" -- {EscapeArgument(subject)}";
    }

    protected override string DeleteCommand(string path)
    {
        return $"rm -- {EscapeArgument(path)}";
    }

    private static string EscapeArgument(string argument)
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
