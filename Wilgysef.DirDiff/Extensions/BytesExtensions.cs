namespace Wilgysef.DirDiff.Extensions;

internal static class BytesExtensions
{
    private static readonly uint[] _hexLookupLowercase32 = CreateLookupLowercase32();
    private static readonly uint[] _hexLookupUppercase32 = CreateLookupUppercase32();

    // https://stackoverflow.com/a/24343727
    public static string ToHex(this byte[] bytes, bool lowerCase = true)
    {
        var lookup32 = lowerCase ? _hexLookupLowercase32 : _hexLookupUppercase32;
        var result = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            var val = lookup32[bytes[i]];
            result[2 * i] = (char)val;
            result[2 * i + 1] = (char)(val >> 16);
        }
        return new string(result);
    }

    private static uint[] CreateLookupLowercase32()
    {
        var result = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString("x2");
            result[i] = s[0] + ((uint)s[1] << 16);
        }
        return result;
    }

    private static uint[] CreateLookupUppercase32()
    {
        var result = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString("X2");
            result[i] = s[0] + ((uint)s[1] << 16);
        }
        return result;
    }
}
