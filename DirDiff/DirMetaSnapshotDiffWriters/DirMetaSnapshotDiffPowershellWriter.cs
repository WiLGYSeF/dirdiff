using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffPowershellWriter : DirMetaSnapshotDiffCommandWriter
{
    public override DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    protected override string CopyCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject)
    {
        return CopyCommand(reference.Path, subject.Path);
    }

    protected override string CopyCommand(string reference, string subject)
    {
        return $"Copy-Item -LiteralPath {EscapeArgument(reference)} -Destination {EscapeArgument(subject)}";
    }

    protected override string MoveCommand(string oldPath, string newPath)
    {
        return $"Move-Item -LiteralPath {EscapeArgument(oldPath)} -Destination {EscapeArgument(newPath)}";
    }

    protected override string TouchCommand(DirMetaSnapshotEntry reference, DirMetaSnapshotEntry subject)
    {
        return $"(Get-ChildItem -LiteralPath {EscapeArgument(subject.Path)}).LastWriteTime = (Get-ChildItem -LiteralPath {EscapeArgument(reference.Path)}).LastWriteTime";
    }

    protected override string DeleteCommand(DirMetaSnapshotEntry entry)
    {
        return $"Remove-Item -LiteralPath {EscapeArgument(entry.Path)}";
    }

    private string EscapeArgument(string argument)
    {
        var builder = new StringBuilder();
        builder.Append('"');

        for (var i = 0; i < argument.Length; i++)
        {
            switch (argument[i])
            {
                case '"':
                    builder.Append("`\"");
                    break;
                default:
                    builder.Append(argument[i]);
                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }
}
