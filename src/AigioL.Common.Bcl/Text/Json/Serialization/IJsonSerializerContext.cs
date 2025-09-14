using System.Text.Encodings.Web;

namespace System.Text.Json.Serialization;

#if !NETFRAMEWORK
public partial interface IJsonSerializerContext
#else
public static partial class IJsonSerializerContext
#endif
{
    public const string TypeDiscriminatorPropertyName = "$tag";

#if !NETFRAMEWORK
    /// <summary>
    /// 返回 Json 源生成的 <see cref="JsonSerializerContext"/> 默认实例
    /// </summary>
    static abstract JsonSerializerContext GetDefault();

    public static JsonSerializerOptions GetJsonSerializerOptions<T>(bool writeIndented = false) where T : IJsonSerializerContext
    {
        try
        {
            var opt = T.GetDefault().Options;
            if (writeIndented)
            {
                if (opt.WriteIndented)
                {
                    return opt;
                }
                else
                {
                    opt = new(opt) // 重新创建一份，不修改原值
                    {
                        WriteIndented = true,
                    };
                    return opt;
                }
            }
            else
            {
                if (opt.WriteIndented)
                {
                    opt = new(opt) // 重新创建一份，不修改原值
                    {
                        WriteIndented = false,
                    };
                    return opt;
                }
                else
                {
                    return opt;
                }
            }
        }
        catch
        {
            // 如果模型类实现接口 IJsonSerializerContext 的静态函数，则不可能进入此处
            // 由模型类实现的函数使用 JSON 源生成的 Options，否则将回退到默认的 Web 选项
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            return JsonSerializerOptions.Web;
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }
    }
#endif

    public static void SetDefaultOptions(JsonSerializerOptions o)
    {
        // https://github.com/dotnet/runtime/issues/94135
        o.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // 不转义字符！！！
        o.AllowTrailingCommas = true;

        #region JsonSerializerDefaults.Web https://github.com/dotnet/runtime/blob/v9.0.7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L172-L174

        o.PropertyNameCaseInsensitive = true; // 忽略大小写
        o.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // 驼峰命名
        o.NumberHandling = JsonNumberHandling.AllowReadingFromString; // 允许从字符串读取数字

        #endregion

        o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // 忽略 null 值
    }
}