using AigioL.Common.TencentCloud.Sdk.IM.Helpers;
using AigioL.Common.TencentCloud.Sdk.IM.Models.Abstractions;

namespace AigioL.Common.UnitTest.TencentCloud;

public sealed class TLSSigAPIv2Test : BaseUnitTest
{
    sealed record class TLSSigAPIv2AppIdWithKeyModel(string SdkAppId, string Key) : ITLSSigAPIv2AppIdWithKey;

    /// <summary>
    /// 生成随机数字，长度为固定传入参数
    /// </summary>
    /// <param name="length">要生成的字符串长度</param>
    /// <param name="endIsZero">生成的数字最后一位是否能够为0，默认不能为0( <see langword="false"/> )</param>
    /// <returns></returns>
    static int GenerateRandomNum(int length = 6, bool endIsZero = false)
    {
        if (length > 11) length = 11;
        var random = Random.Shared;
        var result = 0;
        var lastNum = 0;
        if (random.Next(256) % 2 == 0)
            for (int i = length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(i);
        else
            for (int i = 0; i < length; i++) // 0 1 2 3 4 5
                EachGenerate(i);
        return result;
        void EachGenerate(int i)
        {
            var bit = (int)(i == 0 ? 1 : Math.Pow(10, i));
            // 100,000  10,000  1,000   100     10      1
            // 1        10      100     1,000   10,000  100,000
            var current = random.Next(lastNum + 1, lastNum + 10);
            lastNum = current % 10;
            if (lastNum == 0)
            {
                // i != 0 &&  i!=5 末尾和开头不能有零
                if ((i != 0 || endIsZero) && i != length - 1)
                    return;
                lastNum = random.Next(1, 10);
            }
            result += lastNum * bit;
        }
    }

    static ITLSSigAPIv2AppIdWithKey GetOptions()
    {
        TLSSigAPIv2AppIdWithKeyModel m = new(GenerateRandomNum(uint.MaxValue.ToString().Length - 1).ToString(), Guid.CreateVersion7().ToString("N"));
        return m;
    }

    [Fact]
    public void GenUserSig()
    {
        var opt = GetOptions();
        var userId = Guid.CreateVersion7().ToString("N");
        var r = TLSSigAPIv2.GenUserSig(opt, userId);
        Console.WriteLine(r);
    }

    [Fact]
    public void GenPrivateMapKey()
    {
        var opt = GetOptions();
        var userId = Guid.CreateVersion7().ToString("N");
        var expire = Random.Shared.Next();
        var roomid = unchecked((uint)Random.Shared.Next());
        var privilegeMap = unchecked((uint)Random.Shared.Next());
        var r = TLSSigAPIv2.GenPrivateMapKey(opt, userId, expire, roomid, privilegeMap);
        Console.WriteLine(r);
    }

    [Fact]
    public void GenPrivateMapKeyWithStringRoomID()
    {
        var opt = GetOptions();
        var userId = Guid.CreateVersion7().ToString("N");
        var expire = Random.Shared.Next();
        var roomstr = GenerateRandomNum(255).ToString();
        var privilegeMap = unchecked((uint)Random.Shared.Next());
        var r = TLSSigAPIv2.GenPrivateMapKeyWithStringRoomID(opt, userId, expire, roomstr, privilegeMap);
        Console.WriteLine(r);
    }
}
