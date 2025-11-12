using AigioL.Common.JsonWebTokens.Models.Abstractions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AigioL.Common.JsonWebTokens.Services.Abstractions;

/// <summary>
/// JsonWebToken 值提供者默认实现基类
/// </summary>
public abstract class JsonWebTokenValueProviderBase<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>
    where TOptions : IJsonWebTokenOptions
{
    /// <summary>
    /// 随机数生成器
    /// </summary>
    protected static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    protected abstract TOptions GetOptions();

    SigningCredentials? signingCredentials;

    protected SigningCredentials GetSigningCredentials()
    {
        if (signingCredentials == null)
        {
            var o = GetOptions();
            var securityKey = IJsonWebTokenOptions.GetSymmetricSecurityKey(o);
            signingCredentials = new SigningCredentials(securityKey, o.SecretAlgorithm);
        }
        return signingCredentials;
    }

    /// <summary>
    /// 向 Claims 中添加角色信息
    /// </summary>
    protected static void AddRolesToClaims(IList<Claim> claims, params IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            var roleClaim = new Claim(ClaimTypes.Role, role);
            claims.Add(roleClaim);
        }
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    protected static string GenerateRefreshToken(string password)
    {
        // https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs#L141
        const int saltSize = 32;
        const int iterCount = 10000;
        const KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256;
        const int numBytesRequested = 32;
        var salt = new byte[saltSize];
        _rng.GetBytes(salt);
        var subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subkey.Length];
        outputBytes[0] = 0x01; // format marker
        WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
        WriteNetworkByteOrder(outputBytes, 5, iterCount);
        WriteNetworkByteOrder(outputBytes, 9, saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);

        var result = Base64Url.EncodeToString(outputBytes);
        return result;
    }

    /// <summary>
    /// 将无符号整型值以网络字节顺序写入字节数组中
    /// </summary>
    protected static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }
}