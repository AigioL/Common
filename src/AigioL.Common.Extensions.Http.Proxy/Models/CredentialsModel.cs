using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

/// <summary>
/// <see cref="ICredentials"/> 的可序列化模型基类
/// </summary>
[JsonDerivedType(typeof(CredentialCacheModel), typeDiscriminator: "CredentialCache")]
[JsonDerivedType(typeof(NetworkCredentialModel), typeDiscriminator: "NetworkCredential")]
public abstract partial record class CredentialsModel
{
    public abstract ICredentials? GetCredentials();

    public override string ToString()
    {
        // 已使用源生成器生成的上下文进行序列化
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        var str = JsonSerializer.Serialize(this, ExHttpProxyJsonSerializerContext.Default.Options);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        return str;
    }
}
