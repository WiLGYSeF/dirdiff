using System.Security.Cryptography;

namespace Wilgysef.DirDiff.Hashers;

public class Hasher : IHasher
{
    public async Task<byte[]?> HashStreamAsync(Enums.HashAlgorithm algorithm, Stream stream)
    {
        byte[] hash = algorithm switch
        {
            Enums.HashAlgorithm.MD5 => await HashStreamMd5Async(stream),
            Enums.HashAlgorithm.SHA1 => await HashStreamSha1Async(stream),
            Enums.HashAlgorithm.SHA256 => await HashStreamSha256Async(stream),
            Enums.HashAlgorithm.SHA384 => await HashStreamSha384Async(stream),
            Enums.HashAlgorithm.SHA512 => await HashStreamSha512Async(stream),
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

    private static async Task<byte[]> HashStreamMd5Async(Stream stream)
    {
        return await HashStreamAsync(MD5.Create(), stream);
    }

    private static async Task<byte[]> HashStreamSha1Async(Stream stream)
    {
        return await HashStreamAsync(SHA1.Create(), stream);
    }

    private static async Task<byte[]> HashStreamSha256Async(Stream stream)
    {
        return await HashStreamAsync(SHA256.Create(), stream);
    }

    private static async Task<byte[]> HashStreamSha384Async(Stream stream)
    {
        return await HashStreamAsync(SHA384.Create(), stream);
    }

    private static async Task<byte[]> HashStreamSha512Async(Stream stream)
    {
        return await HashStreamAsync(SHA512.Create(), stream);
    }

    private static async Task<byte[]> HashStreamAsync(HashAlgorithm algorithm, Stream stream)
    {
        return await algorithm.ComputeHashAsync(stream);
    }
}
