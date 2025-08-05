using System.Security.Cryptography;

namespace AigioL.Common.UnitTest;

public sealed class HashsTest : BaseUnitTest
{
    [Theory]
    [InlineData(HashAlgorithmTypeName.MD5)]
    [InlineData(HashAlgorithmTypeName.SHA1)]
    [InlineData(HashAlgorithmTypeName.SHA256)]
    [InlineData(HashAlgorithmTypeName.SHA384)]
    [InlineData(HashAlgorithmTypeName.SHA512)]
    //[InlineData(HashAlgorithmTypeName.SHA3_256)]
    //[InlineData(HashAlgorithmTypeName.SHA3_384)]
    //[InlineData(HashAlgorithmTypeName.SHA3_512)]
#if USE_IO_HASHING
    [InlineData(HashAlgorithmTypeName.Crc32)]
    [InlineData(HashAlgorithmTypeName.Crc64)]
#endif
    public void GetHashData(HashAlgorithmTypeName type)
    {
        Guid guid = Guid.CreateVersion7();
        var b = guid.ToByteArray();
        var s = new MemoryStream(b, false);
        var hash = Hashs.HashDataToHexString(type, s);
        Console.WriteLine($"type: {type}");
        Console.WriteLine($"len: {hash.Length}, hash: {hash}");
    }
}
