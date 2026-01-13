using System.Diagnostics;

namespace AigioL.Common.FeishuOApi.Sdk.Models;

/// <summary>
/// 飞书开放平台配置
/// </summary>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed partial record class FeishuApiOptions
{
    string DebuggerDisplay() => $"HookId: {HookId}, ServerTag: {ServerTag}";

    /// <summary>
    /// 飞书 WebHook Id
    /// </summary>
    public string? HookId { get; set; }

    /// <summary>
    /// 服务标识
    /// </summary>
    public partial string? ServerTag { get; set; }
}

#if !_IGNORE_PROGRAM_HELPER
partial record class FeishuApiOptions
{
    public partial string? ServerTag
    {
        get
        {
            if (string.Equals(field, "{ProjectId}", StringComparison.InvariantCultureIgnoreCase))
            {
                return global::AigioL.Common.AspNetCore.Helpers.ProgramMain.ProgramHelper.ProjectId;
            }
            return field;
        }
        set => field = value;
    }
}
#else
partial record class FeishuApiOptions
{
    public partial string? ServerTag
    {
        get => field;
        set => field = value;
    }
}
#endif
