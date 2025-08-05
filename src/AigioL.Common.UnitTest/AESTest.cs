using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.UnitTest;

public sealed class AESTest : BaseUnitTest
{
    [Fact]
    public void CreateTest()
    {
        var guid_v7 = Guid.CreateVersion7();
        Console.WriteLine($"guid_v7: {guid_v7}");
        var dt = ShortGuid.GetDateTimeOffset(guid_v7).ToLocalTime();
        Console.WriteLine($"dt: {dt}");

        var str = typeof(AESTest).FullName!;

        using var aes = AESUtils.Create(guid_v7);
        var encrypt = aes.EncryptCbc(Encoding.UTF8.GetBytes(str), aes.IV);
        Console.WriteLine($"encrypt: {Convert.ToHexString(encrypt)}");

        var decrypt = aes.DecryptCbc(encrypt, aes.IV);

        Assert.True(str == Encoding.UTF8.GetString(decrypt));
    }
}