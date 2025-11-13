using AigioL.Common.AspNetCore.AppCenter.Security;
using Microsoft.AspNetCore.Http.Metadata;
using MediaTypeNames = AigioL.Common.AspNetCore.AppCenter.Constants.MediaTypeNames;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for adding routing metadata to endpoint instances using <see cref="IEndpointConventionBuilder"/>.
/// </summary>
public static partial class RoutingEndpointConventionBuilderExtensions
{
    static readonly AcceptsMetadata acceptsMetadata = new(
        [
            MediaTypeNames.JSON,
            MediaTypeNames.JSONSecurity,
            MediaTypeNames.JSONSecurityECDiffieHellman,
            MediaTypeNames.MemoryPack,
            MediaTypeNames.MemoryPackSecurity,
            MediaTypeNames.MemoryPackSecurityECDiffieHellman,
        ],
        typeof(object),
        true
    );

    /// <summary>
    /// 标注终结点元数据，表示该终结点必须使用 SecurityKey 模式
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="algorithmType"></param>
    /// <returns></returns>
    public static TBuilder WithRequiredSecurityKey<TBuilder>(this TBuilder builder,
        SecurityKeyAlgorithmType algorithmType = RequiredSecurityKeyAttribute.DefaultAlgorithmType)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new RequiredSecurityKeyAttribute(algorithmType));
            endpointBuilder.Metadata.Add(acceptsMetadata);
        });
        return builder;
    }
}
