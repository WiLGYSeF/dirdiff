using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Extensions;
using DirDiff.Tests.Utils;
using System.Text;

namespace DirDiff.Tests.DirMetaSnapshotWritersTests;

public class DirMetaSnapshotTextWriterTest
{
    [Fact]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder().Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure((DirMetaSnapshotTextWriterOptions options) =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var content = Encoding.UTF8.GetString(stream.ToArray());
        var lines = content.Split(Environment.NewLine)[..^1];

        lines.Length.ShouldBe(entries.Count);

        var linesEnumerator = lines.GetEnumerator();
        var entriesEnumerator = entries.GetEnumerator();

        while (linesEnumerator.MoveNext() && entriesEnumerator.MoveNext())
        {
            var entry = entriesEnumerator.Current;

            var expected = new string[]
            {
                entry.HashHex!,
                entry.HashAlgorithm!.Value.ToEnumMemberValue(),
                Math.Floor(entry.CreatedTime!.Value.ToUnixTimestamp()).ToString(),
                Math.Floor(entry.LastModifiedTime!.Value.ToUnixTimestamp()).ToString(),
                entry.FileSize!.Value.ToString(),
                entry.Path,
            }.JoinAsString(writer.TextWriterOptions.Separator);

            linesEnumerator.Current.ShouldBe(expected);
        }
    }

    [Fact]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize_NoValue()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .WithNoHash()
                .WithCreatedTime(null)
                .WithLastModifiedTime(null)
                .WithFileSize(null)
                .Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure((DirMetaSnapshotTextWriterOptions options) =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var content = Encoding.UTF8.GetString(stream.ToArray());
        var lines = content.Split(Environment.NewLine)[..^1];

        lines.Length.ShouldBe(entries.Count);

        var linesEnumerator = lines.GetEnumerator();
        var entriesEnumerator = entries.GetEnumerator();

        while (linesEnumerator.MoveNext() && entriesEnumerator.MoveNext())
        {
            var entry = entriesEnumerator.Current;

            var expected = new string[]
            {
                writer.TextWriterOptions.NoneValue,
                writer.TextWriterOptions.NoneValue,
                writer.TextWriterOptions.NoneValue,
                writer.TextWriterOptions.NoneValue,
                writer.TextWriterOptions.NoneValue,
                entry.Path,
            }.JoinAsString(writer.TextWriterOptions.Separator);

            linesEnumerator.Current.ShouldBe(expected);
        }
    }

    [Fact]
    public async Task Write_Hash_LastModifiedTime_FileSize()
    {
        var stream = new MemoryStream();

        var snapshot = new DirMetaSnapshot();
        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            var entry = new DirMetaSnapshotEntryBuilder()
                .Build();
            snapshot.AddEntry(entry);
            entries.Add(entry);
        }

        var writer = new DirMetaSnapshotTextWriter()
            .Configure((DirMetaSnapshotTextWriterOptions options) =>
            {
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var content = Encoding.UTF8.GetString(stream.ToArray());
        var lines = content.Split(Environment.NewLine)[..^1];

        lines.Length.ShouldBe(entries.Count);

        var linesEnumerator = lines.GetEnumerator();
        var entriesEnumerator = entries.GetEnumerator();

        while (linesEnumerator.MoveNext() && entriesEnumerator.MoveNext())
        {
            var entry = entriesEnumerator.Current;

            var expected = new string[]
            {
                entry.HashHex!,
                Math.Floor(entry.LastModifiedTime!.Value.ToUnixTimestamp()).ToString(),
                entry.FileSize!.Value.ToString(),
                entry.Path,
            }.JoinAsString(writer.TextWriterOptions.Separator);

            linesEnumerator.Current.ShouldBe(expected);
        }
    }
}
