using DirDiff.Enums;

namespace DirDiff.Hashers;

public interface IHasher
{
    Task<byte[]?> HashStreamAsync(HashAlgorithm algorithm, Stream stream);
}
