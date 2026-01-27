using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

/// <summary>
/// <see cref="NetworkCredential"/> 的可序列化模型类
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Sequential)]
public partial record class NetworkCredentialModel : CredentialsModel
{
    /// <inheritdoc cref="NetworkCredential.Domain"/>
    public string? Domain { get; set; }

    /// <inheritdoc cref="NetworkCredential.Password"/>
    public string? Password { get; set; }

    /// <inheritdoc cref="NetworkCredential.UserName"/>
    public string? UserName { get; set; }

    public override ICredentials? GetCredentials()
    {
        var credential = new NetworkCredential(UserName, Password, Domain);
        return credential;
    }
}
