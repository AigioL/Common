using System.Runtime.Versioning;

namespace System.Security.Cryptography;

public enum HashAlgorithmTypeName : byte
{
    MD5 = 1,
    SHA1,

    SHA256,
    SHA384,
    SHA512,

    [SupportedOSPlatform("windows10.0.25324")]
    SHA3_256,
    [SupportedOSPlatform("windows10.0.25324")]
    SHA3_384,
    [SupportedOSPlatform("windows10.0.25324")]
    SHA3_512,

    Crc32,
    Crc64,
}
