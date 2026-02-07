using AigioL.Common.Models.Abstractions;
using AngleSharp.Dom;
using System.Net;

namespace AigioL.Common.Models;

public static partial class ApiRspExtensions
{
    public static Func<Exception, uint>? GetCodeByExceptionDelegate { private get; set; }

    public static void SetException(this ApiRsp apiRsp, Exception ex)
    {
        var allMsg = ex.GetAllMessage();
        if (ex is ApiRspCodeException apiRspCodeException)
        {
            apiRsp.Code = apiRspCodeException.GetCode();
        }
        else
        {
            var d = GetCodeByExceptionDelegate;
            apiRsp.Code = d != null ? d(ex) : unchecked((uint)HttpStatusCode.InternalServerError);
        }
        apiRsp.Message = allMsg;
    }

    public static void SetIsSuccess(this ApiRsp apiRsp, bool isSuccess)
    {
        apiRsp.Code = isSuccess ?
                unchecked((uint)HttpStatusCode.OK) :
                unchecked((uint)HttpStatusCode.BadRequest);
    }

    public static ApiRsp<TContent?> CreateNew<TContent>(this ApiRsp apiRsp, TContent? content = default)
    {
        ApiRsp<TContent?> r = new()
        {
            Code = apiRsp.Code,
            Message = apiRsp.Message,
            Url = apiRsp.Url,
            TraceId = apiRsp.TraceId,
            Content = content,
        };
        return r;
    }
}