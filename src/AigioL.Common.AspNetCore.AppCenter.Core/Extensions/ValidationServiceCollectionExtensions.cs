#if NET10_0_OR_GREATER
#else
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static class ValidationServiceCollectionExtensions
{
    [Obsolete("在 < .NET 10 中不可用，https://learn.microsoft.com/zh-cn/dotnet/core/extensions/options-validation-generator")]
    public static IServiceCollection AddValidation(
        this IServiceCollection services)
    {
        // 仅支持 .NET 10+
        return services;
    }
}
#endif