namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 管理后台按钮类型
/// </summary>
public enum BMButtonType : byte
{
    /// <summary>
    /// 编辑按钮
    /// </summary>
    Edit = 1,

    /// <summary>
    /// 删除按钮
    /// </summary>
    Delete = 2,

    /// <summary>
    /// 查看详情按钮
    /// </summary>
    Detail = 3,

    /// <summary>
    /// 新增按钮
    /// </summary>
    Add = 4,

    /// <summary>
    /// 查询按钮
    /// </summary>
    Query = 5,

    /// <summary>
    /// 其他自定义按钮
    /// </summary>
    Other = 10,
}
