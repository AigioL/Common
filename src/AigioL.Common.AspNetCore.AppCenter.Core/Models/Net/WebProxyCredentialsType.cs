namespace AigioL.Common.AspNetCore.AppCenter.Models.Net;

public enum WebProxyCredentialsType : byte
{
    /// <summary>
    /// <see langword="null"/>
    /// </summary>
    Null,

    /// <summary>
    /// <see cref="global::System.Net.NetworkCredential"/>
    /// </summary>
    NetworkCredential,

    /// <summary>
    /// <see cref="global::System.Net.CredentialCache"/>
    /// </summary>
    CredentialCache,
}
