using AigioL.Common.Models;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

/// <summary>
/// 渠道包服务接口
/// </summary>
public partial interface IChannelPackageService
{
    bool TryParse(string channelPackageId, out Guid channelPackageIdG);

    Task<bool> ExistsAsync(
        Guid channelPackageId,
        CancellationToken cancellationToken = default);

    static bool CheckId(
        [NotNullWhen(true)] IChannelPackageService? channelPackageService,
        string channelPackageId,
        out Guid? channelPackageIdGN,
        out ApiRspCode code)
    {
        channelPackageIdGN = null;
        if (channelPackageService == null)
        {
            code = ApiRspCode.ChannelPackageServiceIsNull;
            return false;
        }

        if (channelPackageService.TryParse(channelPackageId, out Guid channelPackageIdG) && channelPackageIdG != default)
        {
            channelPackageIdGN = channelPackageIdG;
        }

        code = ApiRspCode.OK;
        return true;
    }
}
