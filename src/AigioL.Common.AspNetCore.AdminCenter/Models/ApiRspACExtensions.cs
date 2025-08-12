using AigioL.Common.Models.Abstractions;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

public static partial class ApiRspACExtensions
{
    public static void SetException(this ApiRspAC apiRsp, Exception ex)
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

    public static void SetIsSuccess(this ApiRspAC apiRsp, bool isSuccess)
    {
        apiRsp.Code = isSuccess ?
                unchecked((uint)HttpStatusCode.OK) :
                unchecked((uint)HttpStatusCode.BadRequest);
    }
}