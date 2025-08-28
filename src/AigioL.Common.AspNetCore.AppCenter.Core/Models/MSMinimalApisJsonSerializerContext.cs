using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

[JsonSerializable(typeof(ApiRsp))]
[JsonSerializable(typeof(ApiRsp<bool>))]
[JsonSerializable(typeof(ApiRsp<byte>))]
[JsonSerializable(typeof(ApiRsp<sbyte>))]
[JsonSerializable(typeof(ApiRsp<ushort>))]
[JsonSerializable(typeof(ApiRsp<short>))]
[JsonSerializable(typeof(ApiRsp<uint>))]
[JsonSerializable(typeof(ApiRsp<int>))]
[JsonSerializable(typeof(ApiRsp<ulong>))]
[JsonSerializable(typeof(ApiRsp<long>))]
[JsonSerializable(typeof(ApiRsp<Guid>))]
[JsonSerializable(typeof(ApiRsp<float>))]
[JsonSerializable(typeof(ApiRsp<double>))]
[JsonSerializable(typeof(ApiRsp<decimal>))]
[JsonSerializable(typeof(ApiRsp<DateOnly>))]
[JsonSerializable(typeof(ApiRsp<DateTime>))]
[JsonSerializable(typeof(ApiRsp<DateTimeOffset>))]
[JsonSerializable(typeof(ApiRsp<bool?>))]
[JsonSerializable(typeof(ApiRsp<byte?>))]
[JsonSerializable(typeof(ApiRsp<sbyte?>))]
[JsonSerializable(typeof(ApiRsp<ushort?>))]
[JsonSerializable(typeof(ApiRsp<short?>))]
[JsonSerializable(typeof(ApiRsp<uint?>))]
[JsonSerializable(typeof(ApiRsp<int?>))]
[JsonSerializable(typeof(ApiRsp<ulong?>))]
[JsonSerializable(typeof(ApiRsp<long?>))]
[JsonSerializable(typeof(ApiRsp<Guid?>))]
[JsonSerializable(typeof(ApiRsp<float?>))]
[JsonSerializable(typeof(ApiRsp<double?>))]
[JsonSerializable(typeof(ApiRsp<decimal?>))]
[JsonSerializable(typeof(ApiRsp<DateOnly?>))]
[JsonSerializable(typeof(ApiRsp<DateTime?>))]
[JsonSerializable(typeof(ApiRsp<DateTimeOffset?>))]
[JsonSerializable(typeof(ApiRsp<string>))]
[JsonSerializable(typeof(ApiRsp<nil>))]
[JsonSerializable(typeof(ApiRsp<nil?>))]
[JsonSerializable(typeof(ApiRsp<JsonWebTokenValue>))]
[JsonSourceGenerationOptions]
public sealed partial class MSMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static MSMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new MSMinimalApisJsonSerializerContext(o);
    }
}
