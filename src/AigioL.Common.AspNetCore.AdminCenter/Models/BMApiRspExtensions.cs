using AigioL.Common.Models.Abstractions;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

public static partial class BMApiRspExtensions
{
    public static void SetException(this BMApiRsp apiRsp, Exception ex)
    {
        var allMsg = ex.GetAllMessage();
        if (ex is ApiRspCodeException apiRspCodeException)
        {
            apiRsp.Code = apiRspCodeException.GetCode();
        }
        else
        {
            apiRsp.Code = unchecked((uint)HttpStatusCode.InternalServerError);
        }
        apiRsp.Messages = [allMsg];
    }

    public static void SetIsSuccess(this BMApiRsp apiRsp, bool isSuccess)
    {
        apiRsp.Code = isSuccess ?
                unchecked((uint)HttpStatusCode.OK) :
                unchecked((uint)HttpStatusCode.BadRequest);
    }
}