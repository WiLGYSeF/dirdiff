using DirDiff.Enums;
using System.Text;

namespace DirDiff.Tests.Utils;

internal static class TestUtils
{
    private static readonly Random _random = new();

    private static readonly char[] RandomStringDefaultCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    #region Random

    public static int RandomInt()
    {
        return _random.Next();
    }

    public static int RandomInt(int max)
    {
        return RandomInt(0, max);
    }

    public static int RandomInt(int min, int max)
    {
        return _random.Next(min, max);
    }

    public static long RandomLong()
    {
        return _random.NextInt64();
    }

    public static long RandomLong(long max)
    {
        return _random.NextInt64(0, max);
    }

    public static long RandomLong(long min, long max)
    {
        return _random.NextInt64(min, max);
    }

    public static double RandomDouble()
    {
        return _random.NextDouble();
    }

    public static double RandomDouble(double max)
    {
        return RandomDouble(0, max);
    }

    public static double RandomDouble(double min, double max)
    {
        return RandomDouble() * (max - min) + min;
    }

    public static byte[] RandomBytes(int length)
    {
        byte[] bytes = new byte[length];
        _random.NextBytes(bytes);
        return bytes;
    }

    public static byte[] RandomBytes(byte[] bytes)
    {
        _random.NextBytes(bytes);
        return bytes;
    }

    public static byte[] RandomHash(HashAlgorithm algorithm)
    {
        return RandomBytes(algorithm switch
        {
            HashAlgorithm.MD5 => 16,
            HashAlgorithm.SHA1 => 20,
            HashAlgorithm.SHA256 => 32,
            HashAlgorithm.SHA384 => 48,
            HashAlgorithm.SHA512 => 64,
            _ => throw new NotImplementedException(),
        });
    }

    public static string RandomString(int length)
    {
        return RandomString(length, RandomStringDefaultCharset);
    }

    public static string RandomString(int length, char[] charset)
    {
        var sbuilder = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            sbuilder.Append(charset[RandomInt(charset.Length)]);
        }
        return sbuilder.ToString();
    }

    public static string RandomPath(int parts, string separator = "/")
    {
        var minPartLength = 4;
        var maxPartLength = 24;

        var sbuilder = new StringBuilder();
        for (var i = 0; i < parts; i++)
        {
            sbuilder.Append(RandomString(RandomInt(minPartLength, maxPartLength)));
            if (i < parts - 1)
            {
                sbuilder.Append(separator);
            }
        }
        return sbuilder.ToString();
    }

    public static string RandomExtension()
    {
        return "." + RandomString(3);
    }

    public static DateTime RandomFileTimestamp()
    {
        return RandomDateTime(new DateTime(2000, 1, 1), new DateTime(2100, 1, 1), DateTimePrecision.Milliseconds)
            .ToUniversalTime();
    }

    public static DateTime RandomDateTime(DateTime start, DateTime end, DateTimePrecision precision)
    {
        return precision switch
        {
            DateTimePrecision.Milliseconds => start + TimeSpan.FromMilliseconds(RandomDouble((end - start).TotalMilliseconds)),
            DateTimePrecision.Seconds => start + TimeSpan.FromSeconds(RandomDouble((end - start).TotalSeconds)),
            DateTimePrecision.Minutes => start + TimeSpan.FromMinutes(RandomDouble((end - start).TotalMinutes)),
            DateTimePrecision.Hours => start + TimeSpan.FromHours(RandomDouble((end - start).TotalHours)),
            DateTimePrecision.Days => start + TimeSpan.FromDays(RandomDouble((end - start).TotalDays)),
            _ => throw new NotImplementedException(),
        };
    }

    public enum DateTimePrecision
    {
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days,
    }

    #endregion
}
