namespace AigioL.Common.AspNetCore.AppCenter.Constants;

/// <summary>
/// MIME 类型
/// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Basics_of_HTTP/MIME_Types</para>
/// </summary>
public static partial class MediaTypeNames
{
    /// <summary>
    /// application/json
    /// </summary>
    public const string JSON = "application/json";

    /// <summary>
    /// application/vnd.sapi+x-json
    /// </summary>
    public const string JSONSecurity = "application/vnd.sapi+x-json";

    /// <summary>
    /// application/vnd.ecdh+x-json
    /// </summary>
    public const string JSONSecurityECDiffieHellman = "application/vnd.ecdh+x-json";

    /// <summary>
    /// application/x-memorypack
    /// </summary>
    public const string MemoryPack = "application/x-memorypack";

    /// <summary>
    /// application/vnd.sapi+x-memorypack
    /// </summary>
    public const string MemoryPackSecurity = "application/vnd.sapi+x-memorypack";

    /// <summary>
    /// application/vnd.ecdh+x-memorypack
    /// </summary>
    public const string MemoryPackSecurityECDiffieHellman = "application/vnd.ecdh+x-memorypack";
}