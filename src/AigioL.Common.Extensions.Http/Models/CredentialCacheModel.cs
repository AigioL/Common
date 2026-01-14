using System.Net;

namespace AigioL.Common.Extensions.Http.Models;

/// <summary>
/// <see cref="CredentialCache"/> 的可序列化模型类
/// </summary>
public partial record class CredentialCacheModel : CredentialsModel
{
    public override ICredentials? GetCredentials()
    {
        throw new NotImplementedException();
    }
}