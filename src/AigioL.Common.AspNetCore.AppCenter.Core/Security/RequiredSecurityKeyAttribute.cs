using System.Security.Authentication;

namespace AigioL.Common.AspNetCore.AppCenter.Security;

/// <summary>
/// 必须使用 SecurityKey 模式的终结点标注或元数据
/// </summary>
/// <param name="algorithmType"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed partial class RequiredSecurityKeyAttribute(
    SecurityKeyAlgorithmType algorithmType = RequiredSecurityKeyAttribute.DefaultAlgorithmType) :
    Attribute
{
    /// <see cref="SecurityKeyAlgorithmType"/>
    public SecurityKeyAlgorithmType AlgorithmType { get; } = algorithmType;

    public const SecurityKeyAlgorithmType DefaultAlgorithmType = SecurityKeyAlgorithmType.RSAWithRandomAes;
}
