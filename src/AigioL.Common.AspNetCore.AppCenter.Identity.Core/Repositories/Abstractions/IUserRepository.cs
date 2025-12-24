using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserRepository
{
}

partial interface IUserRepository // 管理后台
{
    /// <summary>
    /// 后台表格查询
    /// </summary>
    Task<PagedModel<UserTableItem>> QueryAsync(
        Guid? id,
        string? openId,
        UserType? userType,
        string? nickName,
        Gender? gender,
        DateTimeOffset?[]? lastLoginTime,
        bool? isLockout,
        string? phoneNumber,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        bool hidePhoneNumberMiddleFour = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据编辑模型添加或更新一行数据
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(UserEdit model);

    /// <summary>
    /// 根据编辑模型添加或更新一行数据（高级权限）
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<int> UpdateElevatedAsync(UserEdit model);

    /// <summary>
    /// 根据主键获取编辑模型
    /// </summary>
    Task<UserEdit?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索用户
    /// </summary>
    Task<UserSearchModel> SearchUsers(
        string text,
        ushort takeCount = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户钱包详情
    /// </summary>
    Task<UserWalletModel?> GetWalletByUserIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 封禁用户
    /// </summary>
    Task<bool> SetUserLockoutStateAsync(Guid id, bool lockout);
}