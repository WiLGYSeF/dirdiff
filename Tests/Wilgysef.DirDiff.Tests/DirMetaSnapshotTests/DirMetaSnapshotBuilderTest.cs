﻿using Wilgysef.DirDiff.DirMetaSnapshots;
using Wilgysef.DirDiff.DirWalkers;
using Wilgysef.DirDiff.Enums;
using Wilgysef.DirDiff.Extensions;
using Wilgysef.DirDiff.FileInfoReaders;
using Wilgysef.DirDiff.Hashers;
using Wilgysef.DirDiff.Tests.Utils;
using System.Text;

namespace Wilgysef.DirDiff.Tests.DirMetaSnapshotTests;

public class DirMetaSnapshotBuilderTest
{
    [Fact]
    public async Task Create_Snapshot()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entries = new List<DirMetaSnapshotEntry>
        {
            factory.Create().Build(),
            factory.Create().Build(),
            factory.Create().Build(),
            factory.Create().Build()
        };

        var builder = CreateMockedBuilder(entries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        snapshot.Entries.Count.ShouldBe(4);
    }

    [Fact]
    public async Task Update_Snapshot_Created_Deleted_Updated()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entries = new List<DirMetaSnapshotEntry>
        {
            factory.Create().Build(),
            factory.Create().Build(),
            factory.Create().Build(),
            factory.Create().Build()
        };

        var builder = CreateMockedBuilder(entries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntries = new List<DirMetaSnapshotEntry>();

        foreach (var entry in entries.Take(2))
        {
            newEntries.Add(factory.Create(entry).Build());
        }

        var newAddedEntry = factory.Create().Build();
        newEntries.Add(newAddedEntry);

        builder = CreateMockedBuilder(newEntries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
                options.UpdateKeepRemoved = false;
            });

        foreach (var entry in newEntries)
        {
            builder.AddPath(entry.Path);
        }

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Entries.Count.ShouldBe(3);

        foreach (var expected in entries.Take(2))
        {
            ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == expected.Path), expected);
        }

        ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == newAddedEntry.Path), newAddedEntry);
    }

    [Fact]
    public async Task Update_Snapshot_Created_Updated_Keep_Removed()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entries = new List<DirMetaSnapshotEntry>
        {
            factory.Create().Build(),
            factory.Create().Build(),
            factory.Create().Build(),
            factory.Create().Build()
        };

        var builder = CreateMockedBuilder(entries, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntries = new List<DirMetaSnapshotEntry>();
        var otherEntries = new List<DirMetaSnapshotEntry>();

        foreach (var entry in entries.Take(2))
        {
            newEntries.Add(factory.Create(entry).Build());
        }

        foreach (var entry in entries.Skip(2))
        {
            otherEntries.Add(factory.Create(entry).Build());
        }

        var newAddedEntry = factory.Create().Build();
        newEntries.Add(newAddedEntry);

        builder = CreateMockedBuilder(newEntries.Concat(otherEntries), directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
                options.UpdateKeepRemoved = true;
            });

        foreach (var entry in newEntries)
        {
            builder.AddPath(entry.Path);
        }

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Entries.Count.ShouldBe(5);

        foreach (var expected in entries)
        {
            ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == expected.Path), expected);
        }

        ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == newAddedEntry.Path), newAddedEntry);
    }

    [Fact]
    public async Task Update_Snapshot_Updated_Contents()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithRandomLastModifiedTime()
            .WithRandomFileSize()
            .WithRandomHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Entries.Count.ShouldBe(1);

        ShouldBeEntry(newSnapshot.Entries.Single(e => e.Path == newEntry.Path), newEntry);
    }

    [Fact]
    public async Task Update_Snapshot_Old_LastModified_Null()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create()
            .WithLastModifiedTime(null)
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithRandomLastModifiedTime()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.LastModifiedTime.ShouldBe(newEntry.LastModifiedTime);
    }

    [Fact]
    public async Task Update_Snapshot_New_LastModified_Null()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithLastModifiedTime(null)
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.LastModifiedTime.ShouldBe(entry.LastModifiedTime);
    }

    #region Hash Update Tests

    [Fact]
    public async Task Update_Snapshot_Old_Hash_Null()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create()
            .WithNoHash()
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = null;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithRandomHash(HashAlgorithm.SHA256)
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeEquivalentTo(newEntry.Hash);
    }

    [Fact]
    public async Task Update_Snapshot_New_Hash_Null()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeEquivalentTo(entry.Hash);
    }

    [Fact]
    public async Task Update_Snapshot_New_Hash_Null_LastModified_Changed()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithNoHash()
            .WithRandomLastModifiedTime()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldNotBeNull();
        result.Hash.SequenceEqual(entry.Hash!).ShouldBeFalse();
    }

    [Fact]
    public async Task Update_Snapshot_Old_New_Hash_Null()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create()
            .WithNoHash()
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_Snapshot_New_Hash_Null_No_Algorithm()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = null;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeEquivalentTo(entry.Hash);
    }

    [Fact]
    public async Task Update_Snapshot_Old_New_Hash_Null_No_Algorithm()
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create()
            .WithNoHash()
            .Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = null;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithNoHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = null;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Path.ShouldBe(newEntry.Path);
        result.Hash.ShouldBeNull();
    }

    #endregion

    [Fact]
    public async Task Update_Snapshot_Different_DirectorySeparator()
    {
        var firstDirectorySeparator = '/';
        var secondDirectorySeparator = '\\';
        var firstPrefix = string.Join(firstDirectorySeparator, new[] { "abc", "def" }) + firstDirectorySeparator;
        var secondPrefix = string.Join(secondDirectorySeparator, new[] { "abc", "def" }) + secondDirectorySeparator;

        var firstFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = firstDirectorySeparator,
        };
        var secondFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = secondDirectorySeparator,
        };

        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            entries.Add(firstFactory.Create()
                .WithRandomPath(firstPrefix)
                .Build());
        }

        var builder = CreateMockedBuilder(entries, firstDirectorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntries = new List<DirMetaSnapshotEntry>();

        foreach (var entry in entries.Take(2))
        {
            newEntries.Add(secondFactory.Create(entry)
                .WithPath(secondPrefix + snapshot.ChangePathDirectorySeparator(snapshot.PathWithoutPrefix(entry.Path), secondDirectorySeparator))
                .Build());
        }

        var newAddedEntry = secondFactory.Create()
            .WithRandomPath(secondPrefix)
            .Build();
        newEntries.Add(newAddedEntry);

        builder = CreateMockedBuilder(newEntries, secondDirectorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in newEntries)
        {
            builder.AddPath(entry.Path);
        }

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Prefix.ShouldBe(firstPrefix.Replace(firstDirectorySeparator, secondDirectorySeparator));
        newSnapshot.Entries.Count.ShouldBe(3);

        foreach (var expected in entries.Take(2))
        {
            newSnapshot.Entries.ShouldContain(e => e.Path == snapshot.ChangePathDirectorySeparator(expected.Path, newSnapshot.DirectorySeparator));
        }
    }

    [Theory]
    [InlineData('/')]
    [InlineData('\\')]
    public async Task Update_Snapshot_Different_Prefix_DirectorySeparator(char secondDirectorySeparator)
    {
        var firstDirectorySeparator = '/';
        var firstPrefix = string.Join(firstDirectorySeparator, new[] { "abc", "def" }) + firstDirectorySeparator;
        var secondPrefix = string.Join(secondDirectorySeparator, new[] { "test", "123" }) + secondDirectorySeparator;

        var firstFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = firstDirectorySeparator,
        };
        var secondFactory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = secondDirectorySeparator,
        };

        var entries = new List<DirMetaSnapshotEntry>();

        for (var i = 0; i < 5; i++)
        {
            entries.Add(firstFactory.Create()
                .WithRandomPath(firstPrefix)
                .Build());
        }

        var builder = CreateMockedBuilder(entries, firstDirectorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        foreach (var entry in entries)
        {
            builder.AddPath(entry.Path);
        }

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntries = new List<DirMetaSnapshotEntry>();

        foreach (var entry in entries.Take(2))
        {
            newEntries.Add(secondFactory.Create(entry)
                .WithPath(secondPrefix + snapshot.ChangePathDirectorySeparator(snapshot.PathWithoutPrefix(entry.Path), secondDirectorySeparator))
                .Build());
        }

        var newAddedEntry = secondFactory.Create()
            .WithRandomPath(secondPrefix)
            .Build();
        newEntries.Add(newAddedEntry);

        builder = CreateMockedBuilder(newEntries, secondDirectorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
                options.UpdatePrefix = secondPrefix;
            });

        foreach (var entry in newEntries)
        {
            builder.AddPath(entry.Path);
        }

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        newSnapshot.Prefix.ShouldBe(firstPrefix.Replace(firstDirectorySeparator, secondDirectorySeparator));
        newSnapshot.Entries.Count.ShouldBe(3);

        foreach (var expected in entries.Take(2))
        {
            newSnapshot.Entries.ShouldContain(e => e.Path == snapshot.ChangePathDirectorySeparator(expected.Path, newSnapshot.DirectorySeparator));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_Snapshot_TimeWindow(bool useTimeWindow)
    {
        var directorySeparator = '/';

        var factory = new DirMetaSnapshotEntryBuilderFactory()
        {
            DirectorySeparator = directorySeparator,
        };

        var entry = factory.Create().Build();

        var builder = CreateMockedBuilder(new[] { entry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
            });

        builder.AddPath(entry.Path);

        var snapshot = await builder.CreateSnapshotAsync();

        var newEntry = factory.Create(entry)
            .WithLastModifiedTime(entry.LastModifiedTime + TimeSpan.FromSeconds(20))
            .WithRandomHash()
            .Build();

        builder = CreateMockedBuilder(new[] { newEntry }, directorySeparator)
            .Configure(options =>
            {
                options.UseFileSize = true;
                options.UseCreatedTime = true;
                options.UseLastModifiedTime = true;
                options.HashAlgorithm = HashAlgorithm.SHA256;
                options.TimeWindow = useTimeWindow ? TimeSpan.FromMinutes(1) : TimeSpan.Zero;
            });

        builder.AddPath(newEntry.Path);

        var newSnapshot = await builder.UpdateSnapshotAsync(snapshot);

        var result = newSnapshot.Entries.Single();
        result.Hash.ShouldBeEquivalentTo(useTimeWindow ? entry.Hash : newEntry.Hash);
    }

    private static DirMetaSnapshotBuilder CreateMockedBuilder(
        IEnumerable<DirMetaSnapshotEntry> entries,
        char directorySeparator)
    {
        var entryMap = entries.ToDictionary(e => e.Path);

        var walkerMock = new DirWalkerMock(path =>
        {
            var prefix = path.EndsWith(directorySeparator) ? path : path + directorySeparator;
            return entries
                .Where(e => e.Path.StartsWith(prefix) || e.Path == path)
                .Select(e => new DirWalkerResult(e.Path, e.Type));
        });
        var readerMock = new FileReaderMock(path =>
        {
            if (!entryMap.TryGetValue(path, out var entry))
            {
                throw new NotImplementedException();
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(entry.Path));
        });
        var infoReaderMock = new FileInfoReaderMock(path =>
        {
            if (!entryMap.TryGetValue(path, out var entry))
            {
                throw new NotImplementedException();
            }
            return Task.FromResult(new FileInfoResult()
            {
                Length = entry.FileSize,
                CreationTimeUtc = entry.CreatedTime,
                LastWriteTimeUtc = entry.LastModifiedTime,
            });
        });
        var hasherMock = new HasherMock((algorithm, stream) => Task.FromResult<byte[]?>(null));

        var builder = new DirMetaSnapshotBuilder(
            walkerMock,
            readerMock,
            infoReaderMock,
            hasherMock)
                .Configure(options =>
                {
                    options.DirectorySeparator = directorySeparator;
                });

        hasherMock.Hasher = (algorithm, stream) =>
        {
            using var reader = new StreamReader(stream);
            var path = reader.ReadToEnd();
            if (!entryMap.TryGetValue(path, out var entry))
            {
                throw new NotImplementedException();
            }

            if (entry.Hash != null)
            {
                return Task.FromResult<byte[]?>(entry.Hash);
            }

            return Task.FromResult(builder.Options.HashAlgorithm.HasValue
                ? TestUtils.RandomBytes(Hasher.GetHashBytes(builder.Options.HashAlgorithm.Value))
                : null);
        };

        return builder;
    }

    private static void ShouldBeEntry(DirMetaSnapshotEntry entry, DirMetaSnapshotEntry expected)
    {
        entry.Path.ShouldBe(expected.Path);
        entry.Type.ShouldBe(expected.Type);
        entry.FileSize.ShouldBe(expected.FileSize);
        entry.CreatedTime.ShouldBe(expected.CreatedTime);
        entry.LastModifiedTime.ShouldBe(expected.LastModifiedTime);
        entry.HashAlgorithm.ShouldBe(expected.HashAlgorithm);
        entry.Hash.ShouldBeEquivalentTo(expected.Hash);
    }
}
