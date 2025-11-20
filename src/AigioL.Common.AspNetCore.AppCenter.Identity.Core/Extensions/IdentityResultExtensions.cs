using AigioL.Common.Models;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Identity;

public static partial class IdentityResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ApiRsp<T?> Fail<T>(
        this IdentityResult identityResult,
        ApiRspCode code = ApiRspCode.BadRequest)
    {
        var errorMessage = FailCore(identityResult);
        ApiRsp<T?> r = new()
        {
            Code = unchecked((uint)code),
            Message = errorMessage,
        };
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ApiRsp Fail(
        this IdentityResult identityResult,
        ApiRspCode code = ApiRspCode.BadRequest)
    {
        var errorMessage = FailCore(identityResult);
        ApiRsp r = new()
        {
            Code = unchecked((uint)code),
            Message = errorMessage,
        };
        return r;
    }

    static string FailCore(
        IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
        {
            throw new ArgumentOutOfRangeException(nameof(identityResult));
        }
        var error = identityResult.Errors?.FirstOrDefault();
        var errorMessage = "Identity error";
        if (error != null)
        {
            if (!string.IsNullOrWhiteSpace(error.Description))
            {
                errorMessage = error.Description;
            }
            else if (!string.IsNullOrWhiteSpace(error.Code))
            {
                errorMessage = $"Identity error, code: {error.Code}";
            }
        }
        return errorMessage;
    }
}
