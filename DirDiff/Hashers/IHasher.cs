namespace DirDiff.Hashers;

public interface IHasher
{
    byte[]? HashStream(Enums.HashAlgorithm algorithm, Stream stream);
}
