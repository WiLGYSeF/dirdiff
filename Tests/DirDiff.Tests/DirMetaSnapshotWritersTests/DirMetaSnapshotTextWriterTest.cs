﻿using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Extensions;
using DirDiff.Tests.Utils;
using System.Text;

namespace DirDiff.Tests.DirMetaSnapshotWritersTests;

public class DirMetaSnapshotTextWriterTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize(bool writeHeader)
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

        var expectedHeader = "# Hash,HashAlgorithm,CreatedTime,LastModifiedTime,FileSize,Path";

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;

                options.WriteHeader = writeHeader;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var content = Encoding.UTF8.GetString(stream.ToArray());
        var lines = content.Split(Environment.NewLine)[..^1];

        lines.Length.ShouldBe(writeHeader ? entries.Count + 1 : entries.Count);

        var linesEnumerator = lines.GetEnumerator();
        var entriesEnumerator = entries.GetEnumerator();

        if (writeHeader)
        {
            linesEnumerator.MoveNext();
            linesEnumerator.Current.ShouldBe(expectedHeader);
        }

        while (linesEnumerator.MoveNext() && entriesEnumerator.MoveNext())
        {
            var entry = entriesEnumerator.Current;

            var expected = new string[]
            {
                entry.HashHex!,
                entry.HashAlgorithm!.Value.ToEnumMemberValue(),
                ((DateTimeOffset)entry.CreatedTime!.Value).ToUnixTimeSeconds().ToString(),
                ((DateTimeOffset)entry.LastModifiedTime!.Value).ToUnixTimeSeconds().ToString(),
                entry.FileSize!.Value.ToString(),
                entry.Path,
            }.Join(writer.TextWriterOptions.Separator);

            linesEnumerator.Current.ShouldBe(expected);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Write_Hash_HashAlgorithm_CreatedTime_LastModifiedTime_FileSize_NoValue(bool writeHeader)
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

        var expectedHeader = "# Hash,HashAlgorithm,CreatedTime,LastModifiedTime,FileSize,Path";

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteHashAlgorithm = true;
                options.WriteCreatedTime = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;

                options.WriteHeader = writeHeader;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var content = Encoding.UTF8.GetString(stream.ToArray());
        var lines = content.Split(Environment.NewLine)[..^1];

        lines.Length.ShouldBe(writeHeader ? entries.Count + 1 : entries.Count);

        var linesEnumerator = lines.GetEnumerator();
        var entriesEnumerator = entries.GetEnumerator();

        if (writeHeader)
        {
            linesEnumerator.MoveNext();
            linesEnumerator.Current.ShouldBe(expectedHeader);
        }

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
            }.Join(writer.TextWriterOptions.Separator);

            linesEnumerator.Current.ShouldBe(expected);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Write_Hash_LastModifiedTime_FileSize(bool writeHeader)
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

        var expectedHeader = "# Hash,LastModifiedTime,FileSize,Path";

        var writer = new DirMetaSnapshotTextWriter()
            .Configure(options =>
            {
                options.WriteHash = true;
                options.WriteLastModifiedTime = true;
                options.WriteFileSize = true;

                options.WriteHeader = writeHeader;
            });

        await writer.WriteAsync(stream, snapshot);
        stream.Position = 0;

        var content = Encoding.UTF8.GetString(stream.ToArray());
        var lines = content.Split(Environment.NewLine)[..^1];

        lines.Length.ShouldBe(writeHeader ? entries.Count + 1 : entries.Count);

        var linesEnumerator = lines.GetEnumerator();
        var entriesEnumerator = entries.GetEnumerator();

        if (writeHeader)
        {
            linesEnumerator.MoveNext();
            linesEnumerator.Current.ShouldBe(expectedHeader);
        }

        while (linesEnumerator.MoveNext() && entriesEnumerator.MoveNext())
        {
            var entry = entriesEnumerator.Current;

            var expected = new string[]
            {
                entry.HashHex!,
                ((DateTimeOffset)entry.LastModifiedTime!.Value).ToUnixTimeSeconds().ToString(),
                entry.FileSize!.Value.ToString(),
                entry.Path,
            }.Join(writer.TextWriterOptions.Separator);

            linesEnumerator.Current.ShouldBe(expected);
        }
    }
}