using DirDiff.DirMetaSnapshots;
using System.Text;

namespace DirDiff.DirMetaSnapshotDiffWriters;

public abstract class DirMetaSnapshotDiffCommandWriter : IDirMetaSnapshotDiffWriter
{
    abstract public DirMetaSnapshotDiffWriterOptions Options { get; }

    public IDirMetaSnapshotDiffWriter Configure(Action<DirMetaSnapshotDiffWriterOptions> action)
    {
        action(Options);
        return this;
    }

    protected abstract string CopyCommand(string reference, string subject);

    protected abstract string MoveCommand(string oldPath, string newPath);

    protected abstract string TouchCommand(string reference, string subject);

    protected abstract string DeleteCommand(string path);

    public virtual async Task WriteAsync(Stream stream, DirMetaSnapshotDiff diff)
    {
        foreach (var entry in diff.CreatedEntries)
        {
            await WriteCommand(stream, CopyCommand(
                GetPath(diff, entry.Path),
                GetPath(diff, entry.Path, true)));
        }

        foreach (var pair in diff.ModifiedEntries)
        {
            await WriteCommand(stream, CopyCommand(
                GetPath(diff, pair.Second.Path),
                GetPath(diff, pair.First.Path)));
        }

        foreach (var pair in diff.CopiedEntries)
        {
            await WriteCommand(stream, CopyCommand(
                GetPath(diff, pair.First.Path),
                GetPath(diff, pair.Second.Path, true)));
        }

        foreach (var pair in diff.MovedEntries)
        {
            await WriteCommand(stream, MoveCommand(
                GetPath(diff, pair.First.Path),
                GetPath(diff, pair.Second.Path, true)));
        }

        foreach (var pair in diff.TouchedEntries)
        {
            await WriteCommand(stream, TouchCommand(
                GetPath(diff, pair.Second.Path),
                GetPath(diff, pair.First.Path)));
        }

        foreach (var entry in diff.DeletedEntries)
        {
            await WriteCommand(stream, DeleteCommand(GetPath(diff, entry.Path)));
        }
    }

    protected virtual async Task WriteCommand(Stream stream, string command)
    {
        await stream.WriteAsync(Encoding.UTF8.GetBytes(command + Environment.NewLine));
    }

    protected virtual string GetPath(DirMetaSnapshotDiff diff, string path, bool useFirstPrefix = false)
    {
        var snapshot = diff.FirstSnapshot.ContainsPath(path) ? diff.FirstSnapshot : diff.SecondSnapshot;
        var directorySeparator = Options.DirectorySeparator.GetValueOrDefault(diff.SecondSnapshot.DirectorySeparator);
        string? prefix = null;

        if (useFirstPrefix)
        {
            prefix = diff.FirstSnapshot.ChangePathDirectorySeparator(
                Options.FirstPrefix ?? diff.FirstSnapshot.Prefix!,
                directorySeparator);
        }

        if (prefix == null && snapshot == diff.FirstSnapshot && Options.FirstPrefix != null)
        {
            prefix = Options.FirstPrefix;
        }
        if (prefix == null && snapshot == diff.SecondSnapshot && Options.SecondPrefix != null)
        {
            prefix = Options.SecondPrefix;
        }

        if (prefix != null)
        {
            path = prefix + snapshot.PathWithoutPrefix(path);
        }

        return snapshot.ChangePathDirectorySeparator(path, directorySeparator);
    }
}
