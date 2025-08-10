using AigioL.Common.Models.Abstractions;
using System.Net;

namespace AigioL.Common.Models;

public static partial class ApiRspExtensions
{
    public static void SetException(this ApiRsp apiRsp, Exception ex)
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
        apiRsp.Message = allMsg;
    }

    public static void SetIsSuccess(this ApiRsp apiRsp, bool isSuccess)
    {
        apiRsp.Code = isSuccess ?
                unchecked((uint)HttpStatusCode.OK) :
                unchecked((uint)HttpStatusCode.BadRequest);
    }
}