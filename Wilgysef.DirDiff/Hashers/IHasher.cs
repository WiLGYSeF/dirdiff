using Wilgysef.DirDiff.Enums;

namespace Wilgysef.DirDiff.Hashers;

public interface IHasher
{
    Task<byte[]?> HashStreamAsync(HashAlgorithm algorithm, Stream stream);
}
