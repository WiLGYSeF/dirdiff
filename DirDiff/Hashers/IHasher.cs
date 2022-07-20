namespace DirDiff.Hashers;

internal interface IHasher
{
    byte[] HashStream(Enums.HashAlgorithm algorithm, Stream stream);
}
