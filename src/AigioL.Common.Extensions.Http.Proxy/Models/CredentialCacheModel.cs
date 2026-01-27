using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

/// <summary>
/// <see cref="CredentialCache"/> 的可序列化模型类
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Sequential)]
public partial record class CredentialCacheModel : CredentialsModel
{
    public override ICredentials? GetCredentials()
    {
        throw new NotImplementedException();
    }
}