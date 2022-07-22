using DirDiff.Enums;
using DirDiff.Hashers;

namespace DirDiff.Tests.Utils;

internal class HasherMock : IHasher
{
    public Func<HashAlgorithm, Stream, byte[]?> Hasher { get; set; }

    public HasherMock(Func<HashAlgorithm, Stream, byte[]?> hasher)
    {
        Hasher = hasher;
    }

    public byte[]? HashStream(HashAlgorithm algorithm, Stream stream)
    {
        return Hasher(algorithm, stream);
    }
}
