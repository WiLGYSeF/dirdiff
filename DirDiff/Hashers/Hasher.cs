﻿using System.Security.Cryptography;

namespace DirDiff.Hashers;

internal static class Hasher
{
    public static byte[] HashStream(Enums.HashAlgorithm algorithm, Stream stream)
    {
        byte[] hash = algorithm switch
        {
            Enums.HashAlgorithm.MD5 => HashStreamMd5(stream),
            Enums.HashAlgorithm.SHA1 => HashStreamSha1(stream),
            Enums.HashAlgorithm.SHA256 => HashStreamSha256(stream),
            _ => throw new NotImplementedException(),
        };
        return hash;
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

    private static byte[] HashStream(HashAlgorithm algorithm, Stream stream)
    {
        return algorithm.ComputeHash(stream);
    }
}