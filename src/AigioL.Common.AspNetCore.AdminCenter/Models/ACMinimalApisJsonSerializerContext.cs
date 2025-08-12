using AigioL.Common.JsonWebTokens.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

[JsonSerializable(typeof(ApiRspAC))]
[JsonSerializable(typeof(ApiRspAC<bool>))]
[JsonSerializable(typeof(ApiRspAC<byte>))]
[JsonSerializable(typeof(ApiRspAC<sbyte>))]
[JsonSerializable(typeof(ApiRspAC<ushort>))]
[JsonSerializable(typeof(ApiRspAC<short>))]
[JsonSerializable(typeof(ApiRspAC<uint>))]
[JsonSerializable(typeof(ApiRspAC<int>))]
[JsonSerializable(typeof(ApiRspAC<ulong>))]
[JsonSerializable(typeof(ApiRspAC<long>))]
[JsonSerializable(typeof(ApiRspAC<Guid>))]
[JsonSerializable(typeof(ApiRspAC<float>))]
[JsonSerializable(typeof(ApiRspAC<double>))]
[JsonSerializable(typeof(ApiRspAC<decimal>))]
[JsonSerializable(typeof(ApiRspAC<DateOnly>))]
[JsonSerializable(typeof(ApiRspAC<DateTime>))]
[JsonSerializable(typeof(ApiRspAC<DateTimeOffset>))]
[JsonSerializable(typeof(ApiRspAC<bool?>))]
[JsonSerializable(typeof(ApiRspAC<byte?>))]
[JsonSerializable(typeof(ApiRspAC<sbyte?>))]
[JsonSerializable(typeof(ApiRspAC<ushort?>))]
[JsonSerializable(typeof(ApiRspAC<short?>))]
[JsonSerializable(typeof(ApiRspAC<uint?>))]
[JsonSerializable(typeof(ApiRspAC<int?>))]
[JsonSerializable(typeof(ApiRspAC<ulong?>))]
[JsonSerializable(typeof(ApiRspAC<long?>))]
[JsonSerializable(typeof(ApiRspAC<Guid?>))]
[JsonSerializable(typeof(ApiRspAC<float?>))]
[JsonSerializable(typeof(ApiRspAC<double?>))]
[JsonSerializable(typeof(ApiRspAC<decimal?>))]
[JsonSerializable(typeof(ApiRspAC<DateOnly?>))]
[JsonSerializable(typeof(ApiRspAC<DateTime?>))]
[JsonSerializable(typeof(ApiRspAC<DateTimeOffset?>))]
[JsonSerializable(typeof(ApiRspAC<string>))]
[JsonSerializable(typeof(ApiRspAC<nil>))]
[JsonSerializable(typeof(ApiRspAC<nil?>))]
[JsonSerializable(typeof(InitSystemRequest))]
[JsonSerializable(typeof(ApiRspAC<JsonWebTokenValue>))]
[JsonSourceGenerationOptions]
public sealed partial class ACMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static ACMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new ACMinimalApisJsonSerializerContext(o);
    }
}