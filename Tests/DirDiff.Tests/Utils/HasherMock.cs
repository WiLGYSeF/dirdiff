﻿using DirDiff.Enums;
using DirDiff.Hashers;

namespace DirDiff.Tests.Utils;

internal class HasherMock : IHasher
{
    public Func<HashAlgorithm, Stream, Task<byte[]?>> Hasher { get; set; }

    public HasherMock(Func<HashAlgorithm, Stream, Task<byte[]?>> hasher)
    {
        Hasher = hasher;
    }

    public async Task<byte[]?> HashStreamAsync(HashAlgorithm algorithm, Stream stream)
    {
        return await Hasher(algorithm, stream);
    }
}
