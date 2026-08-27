namespace AigioL.Common.Net.ReverseProxy.Constants;

/// <summary>
/// 通用常量静态类
/// </summary>
public static class GeneralConstants
{
    /// <summary>
    /// 通用分隔符
    /// </summary>
    public const char GeneralSeparator = ';';

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    public const int StackallocByteThreshold = 256;
    public const int StackallocCharThreshold = StackallocByteThreshold / 2;
}
