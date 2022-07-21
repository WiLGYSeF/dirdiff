using System.Security.Cryptography;

namespace DirDiff.Hashers;

public class Hasher : IHasher
{
    public byte[] HashStream(Enums.HashAlgorithm algorithm, Stream stream)
    {
        byte[] hash = algorithm switch
        {
            Enums.HashAlgorithm.MD5 => HashStreamMd5(stream),
            Enums.HashAlgorithm.SHA1 => HashStreamSha1(stream),
            Enums.HashAlgorithm.SHA256 => HashStreamSha256(stream),
            Enums.HashAlgorithm.SHA384 => HashStreamSha384(stream),
            Enums.HashAlgorithm.SHA512 => HashStreamSha512(stream),
            _ => throw new NotImplementedException(),
        };
        return hash;
    }

    public static int GetHashBytes(Enums.HashAlgorithm algorithm)
    {
        return algorithm switch
        {
            Enums.HashAlgorithm.MD5 => 16,
            Enums.HashAlgorithm.SHA1 => 20,
            Enums.HashAlgorithm.SHA256 => 32,
            Enums.HashAlgorithm.SHA384 => 48,
            Enums.HashAlgorithm.SHA512 => 64,
            _ => throw new NotImplementedException(),
        };
    }

    private static byte[] HashStreamMd5(Stream stream)
    {
        return HashStream(MD5.Create(), stream);
    }

    private static byte[] HashStreamSha1(Stream stream)
    {
        return HashStream(SHA1.Create(), stream);
    }

    private static byte[] HashStreamSha256(Stream stream)
    {
        return HashStream(SHA256.Create(), stream);
    }

    private static byte[] HashStreamSha384(Stream stream)
    {
        return HashStream(SHA384.Create(), stream);
    }

    private static byte[] HashStreamSha512(Stream stream)
    {
        return HashStream(SHA512.Create(), stream);
    }

    private static byte[] HashStream(HashAlgorithm algorithm, Stream stream)
    {
        return algorithm.ComputeHash(stream);
    }
}
