namespace System.Text.Json.Serialization;

interface IJsonSerializerContext
{
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
}