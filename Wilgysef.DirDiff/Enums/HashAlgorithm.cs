using System.Runtime.Serialization;

namespace Wilgysef.DirDiff.Enums;

public enum HashAlgorithm
{
    [EnumMember(Value = "md5")]
    MD5,
    [EnumMember(Value = "sha1")]
    SHA1,
    [EnumMember(Value = "sha256")]
    SHA256,
    [EnumMember(Value = "sha384")]
    SHA384,
    [EnumMember(Value = "sha512")]
    SHA512,
}
