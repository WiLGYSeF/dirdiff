﻿using DirDiff.DirMetaSnapshots;
using DirDiff.Enums;
using DirDiff.Tests.Utils;

namespace DirDiff.Tests.DirMetaSnapshotTests;

public class DirMetaSnapshotEntryTest
{
    [Fact]
    public void Throws_On_Incorrect_Hash_Length()
    {
        var entry = new DirMetaSnapshotEntry(TestUtils.RandomPath(3), FileType.File)
        {
            HashAlgorithm = HashAlgorithm.SHA256
        };

        Should.Throw<InvalidOperationException>(() => entry.Hash = new byte[] { 1, 2, 3 });

        entry = new DirMetaSnapshotEntry(TestUtils.RandomPath(3), FileType.File)
        {
            Hash = new byte[] { 1, 2, 3 },
        };

        Should.Throw<InvalidOperationException>(() => entry.HashAlgorithm = HashAlgorithm.SHA256);
    }
}
