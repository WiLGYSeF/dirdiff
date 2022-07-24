using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public class DirMetaSnapshotDiffPowershellWriter : DirMetaSnapshotDiffCommandWriter
{
    public override DirMetaSnapshotDiffWriterOptions Options { get; } = new();

    protected override string CopyCommand(string reference, string subject)
    {
        return $"Copy-Item -LiteralPath {EscapeArgument(reference)} -Destination {EscapeArgument(subject)}";
    }

    protected override string MoveCommand(string oldPath, string newPath)
    {
        return $"Move-Item -LiteralPath {EscapeArgument(oldPath)} -Destination {EscapeArgument(newPath)}";
    }

    protected override string TouchCommand(string reference, string subject)
    {
        return $"(Get-ChildItem -LiteralPath {EscapeArgument(subject)}).LastWriteTime = (Get-ChildItem -LiteralPath {EscapeArgument(reference)}).LastWriteTime";
    }

    protected override string DeleteCommand(string path)
    {
        return $"Remove-Item -LiteralPath {EscapeArgument(path)}";
    }

    private static string EscapeArgument(string argument)
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
