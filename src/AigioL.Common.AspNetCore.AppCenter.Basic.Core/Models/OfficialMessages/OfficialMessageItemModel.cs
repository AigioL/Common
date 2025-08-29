namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class OfficialMessageItemModel
{
    /// <summary>
    /// 标题
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息链接
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? MessageLink { get; set; }

    /// <summary>
    /// 推送时间/消息日期
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public DateTimeOffset PushTime { get; set; }
}
