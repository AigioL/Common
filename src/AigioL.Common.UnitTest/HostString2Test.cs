#if !NETFRAMEWORK
using Microsoft.AspNetCore.Http;

namespace AigioL.Common.UnitTest;

/// <summary>
/// 测试 <see cref="HostString"/> 相关的
/// </summary>
public sealed class HostString2Test
{
    /// <summary>
    /// 测试通过 UnsafeAccessor 访问 <see cref="HostString"/> 私有字段 s_safeHostStringChars
    /// </summary>
    [Fact]
    public void GetSafeHostStringCharsTest()
    {
        var temp = HostString2.GetSafeHostStringChars();
        Console.WriteLine(temp);
    }

    /// <summary>
    /// 测试通过 UnsafeAccessor 访问 <see cref="HostString"/> 私有字段 s_idnMapping
    /// </summary>
    [Fact]
    public void GetIdnMappingTest()
    {
        var temp = HostString2.GetIdnMapping();
        Console.WriteLine(temp);
    }
}
#endif