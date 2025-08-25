using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace AigioL.Common.AspNetCore.AdminCenter.Repositories;

sealed partial class BMMenuRepository<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    Repository<TDbContext, BMMenu, Guid>,
    IBMMenuRepository
    where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>
    where TUser : BMUser
    where TRole : BMRole
    where TUserRole : BMUserRole
{
    public BMMenuRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<List<BMMenuTreeItem>> GetTreeAsync()
    {
        // 查询顶级菜单（带上它的下一级子菜单列表）
        var query = db.Menus.AsNoTrackingWithIdentityResolution()
            .Where(static x => !x.ParentId.HasValue)
            .Select(static x => new BMMenuTreeItem
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                Sort = x.Sort,
                Key = x.Key,
                Url = x.Url,
                Children = x.Children!.Select(static x => new BMMenuTreeItem
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Name = x.Name,
                    Sort = x.Sort,
                    Key = x.Key,
                    Url = x.Url,
                }).ToList(),
            });

#if DEBUG
        var sql = query.ToQueryString();
#endif

        var r = await query.ToListAsync(RequestAborted);
        r.Sort(_.menuComparisonBySort);
        r.ForEach(static x => x.Children!.Sort(_.menuComparisonBySort));

        return r;
    }

    public async Task<BMMenuModel?> InfoAsync(Guid id)
    {
        var r = await db.Menus.AsNoTrackingWithIdentityResolution()
           .Select(_.MenuExpr)
           .FirstOrDefaultAsync(x => x.Id == id, RequestAborted);
        return r;
    }

    public async Task<(int rowCount, DbRowExecResult result)> InsertOrUpdateAsync(BMMenuEdit model, Guid userId, Guid tenantId)
    {
        var r = await InsertOrUpdateAsync(model, onAdd: (entity) =>
        {
            entity.CreateUserId = userId;
            entity.TenantId = tenantId;
        }, onUpdate: (entity) =>
        {
            entity.OperatorUserId = userId;
        });
        return r;
    }
}

partial class BMMenuRepository<TDbContext, TUser, TRole, TUserRole> // 菜单权限
{
    public async Task<bool> EditMenuButtonsAsync(Guid menuId, Guid tenantId, params IEnumerable<Guid> buttons)
    {
        var query = db.Menus.AsNoTrackingWithIdentityResolution();

        var menu = query.FirstOrDefaultAsync(x => x.Id == menuId && x.TenantId == tenantId, RequestAborted);
        if (menu == null)
        {
            return false;
        }

        // 删除此角色全部菜单按钮重新添加
        var delQuery = db.MenuButtons.Where(x => x.MenuId == menuId && x.TenantId == tenantId);
        await delQuery.ExecuteDeleteAsync(CancellationToken.None);

        var buttonsE = buttons.Select(x => new BMMenuButton
        {
            MenuId = menuId,
            ButtonId = x,
            TenantId = tenantId,
        });
        await db.MenuButtons.AddRangeAsync(buttonsE, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<List<BMMenuButtonModel>> GetUserMenuAsync(Guid userId, Guid tenantId)
    {
        var q1 = db.Buttons.AsNoTrackingWithIdentityResolution()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new BMButtonModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
            });

        // 按钮字典
        var q2 = from u in db.Users
                 join bur in db.UserRoles on u.Id equals bur.UserId
                 join br in db.Roles on bur.RoleId equals br.Id
                 join bmbr in db.MenuButtonRoles on br.Id equals bmbr.RoleId
                 where u.Id == userId && bur.TenantId == tenantId
                 select bmbr;
        var menuButtons = await q2.Distinct()
            .GroupBy(x => x.MenuId)
            .ToListAsync(RequestAborted);

        var buttonDict = await q1.ToDictionaryAsync(x => x.Id, RequestAborted);
        var q3 = menuButtons.Select(x => new
        {
            x.Key,
            Buttons = x.Select(item => GetMenuSysButtonModel(buttonDict, item.ButtonId))
            .Where(x => x != null && !x.Disable)
            .Select(x => x!),
        });

        var menuButtonDict = q3.ToDictionary(x => x.Key, x => x.Buttons);

        var q4 = db.Menus.AsNoTrackingWithIdentityResolution()
            .Include(x => x.Children!)
            .ThenInclude(x => x.Children)
            .Where(x => !x.ParentId.HasValue)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Sort);

        var menus = await q4.ToListAsync(RequestAborted);

        var q5 = menus.Select(x => MapToTreeModel(menuButtonDict, x))
            .Where(x => x != null && x.Buttons != null)
            .OrderBy(x => x!.Sort)
            .Select(x => x!);
        var r = q5.ToList();
        return r;
    }

    static BMMenuButtonModel? MapToTreeModel(Dictionary<Guid, IEnumerable<BMButtonModel>>? dict, BMMenu m)
    {
        if (dict == null || !dict.TryGetValue(m.Id, out var btn))
            return null;
        if (btn == null)
            return null;
        BMMenuButtonModel r = new()
        {
            Id = m.Id,
            Key = m.Key,
            Name = m.Name,
            Url = m.Url,
            IconUrl = m.IconUrl,
            Sort = m.Sort,
            Buttons = btn?.ToList() ?? [],
            Children = m.Children?.Select(m2 => MapToTreeModel(dict, m2))
                .Where(m2 => m2 != null && m2?.Buttons != null)
                .OrderBy(x => x!.Sort)
                .Select(x => x!)
                .ToList() ?? [],
        };
        return r;
    }

    BMButtonModel? GetMenuSysButtonModel(Dictionary<Guid, BMButtonModel> dict, Guid buttonId)
    {
        if (dict.TryGetValue(buttonId, out var btn))
            return btn;
        return null;
    }

    public async Task<List<BMMenuModel>> GetRoleTreeAsync()
    {
        var query = db.Menus.AsNoTrackingWithIdentityResolution()
            .Include(x => x.Children)
            .Where(x => !x.SoftDeleted)
            .Distinct()
            .OrderBy(x => x.Sort)
            .Select(_.MenuExpr);

        var r = await query.ToListAsync(RequestAborted);
        return r;
    }

    public async Task<List<BMButtonModel>> GetButtonsAsync()
    {
        var query = db.Buttons.AsNoTrackingWithIdentityResolution()
            .Where(x => !x.SoftDeleted)
            .Select(_.BtnExpr);

        var r = await query.ToListAsync(RequestAborted);
        return r;
    }

    public async Task<List<Guid>> GetMenuButtonsAsync(Guid menuId, Guid tenantId)
    {
        var query = db.MenuButtons.AsNoTrackingWithIdentityResolution()
            .Where(x => x.MenuId == menuId && x.TenantId == tenantId);

        var q1 = query.Select(static x => x.ButtonId);

        var r = await q1.ToListAsync(RequestAborted);
        return r;
    }

    public async Task<List<BMButtonModel>> GetRoleMenuButtonsAsync(Guid roleId, Guid menuId, Guid tenantId)
    {
        var query = from button in db.Buttons.AsNoTrackingWithIdentityResolution()
                    join menuRol in db.MenuButtonRoles.AsNoTrackingWithIdentityResolution() on button.Id equals menuRol.ButtonId
                    where !button.SoftDeleted &&
                    menuRol.MenuId == menuId &&
                    menuRol.RoleId == roleId
                    select button;

        query = query.Where(x => x.TenantId == tenantId);

        var r = await query
            .Select(_.BtnExpr).ToListAsync(RequestAborted);
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetControllerName(string controllerName, BMButtonType buttonType) => PermissionAuthorizationRequirement.GetPolicyName(controllerName, buttonType);

    public async Task<bool> AddMenuButtonsAsync(Guid userId, Guid roleId, Guid menuId, Guid tenantId, params IEnumerable<BMButtonModel> buttons)
    {
        var menu = await db.Menus.AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.Id == menuId && x.TenantId == tenantId, RequestAborted);
        if (menu == null)
        {
            return false;
        }
        using var transaction = db.Database.BeginTransaction();
        var isMenuAny = await db.MenuButtonRoles.AsNoTrackingWithIdentityResolution().Where(x =>
                x.MenuId == menuId &&
                x.RoleId == roleId
                && x.TenantId == tenantId
                ).AnyAsync(RequestAborted);
        if (!isMenuAny)
        {
            await db.MenuButtonRoles.AddRangeAsync(buttons.Select(x => new BMMenuButtonRole
            {
                ButtonId = x.Id,
                MenuId = menu.Id,
                RoleId = roleId,
                TenantId = tenantId,
                ControllerName = GetControllerName(menu.Key, x.Type),
            }), CancellationToken.None);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        // 当前菜单有父级 需要判断是否已添加
        if (menu.ParentId.HasValue)
        {
            var isParent = await db.MenuButtonRoles
                .AsNoTrackingWithIdentityResolution()
                .Where(x => x.RoleId == roleId &&
                            x.MenuId == menu.ParentId
                            && x.TenantId == tenantId
                ).AnyAsync(CancellationToken.None);
            if (!isParent)
            {
                var parentMenu = await db.Menus
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefaultAsync(x => x.Id == menu.ParentId.Value && !x.SoftDeleted);
                if (parentMenu == null)
                {
                    return false;
                }
                await db.MenuButtonRoles.AddRangeAsync(buttons.Select(x => new BMMenuButtonRole
                {
                    ButtonId = x.Id,
                    MenuId = menu.ParentId.Value,
                    RoleId = roleId,
                    TenantId = tenantId,
                    ControllerName = GetControllerName(parentMenu.Key, x.Type),
                }), CancellationToken.None);
                await db.SaveChangesAsync(CancellationToken.None);
            }
        }
        else
        {
            // 判断是否父菜单下有其他子菜单
            var isParentMenu = await (from menuItem in db.Menus.AsNoTrackingWithIdentityResolution()
                                      where !menuItem.SoftDeleted &&
                                      menuItem.ParentId == menuId
                                      && menuItem.TenantId == tenantId
                                      select menuItem).AnyAsync(CancellationToken.None);
            // 循环添加子菜单
            if (isParentMenu)
            {
                var query = (from menuItem in db.Menus.AsNoTrackingWithIdentityResolution()
                             where
                             !menuItem.SoftDeleted &&
                             menuItem.ParentId == menuId
                             && menuItem.TenantId == tenantId
                             select menuItem).Distinct();

                var menus = await query.ToArrayAsync(CancellationToken.None);
                List<BMMenuButtonRole> roles = new();
                foreach (var item in menus)
                {
                    var isAny = await db.MenuButtonRoles.Where(x =>
                    x.MenuId == item.Id &&
                    x.RoleId == roleId
                    && x.TenantId == tenantId
                    ).AnyAsync(CancellationToken.None);
                    // 查询不存在则添加
                    if (!isAny)
                    {
                        roles.AddRange(buttons.Select(x => new BMMenuButtonRole
                        {
                            ButtonId = x.Id,
                            MenuId = item.Id,
                            ControllerName = GetControllerName(item.Key, x.Type),
                            TenantId = tenantId,
                            RoleId = roleId,
                        }));
                    }
                }
                await db.MenuButtonRoles.AddRangeAsync(roles, CancellationToken.None);
                await db.SaveChangesAsync(CancellationToken.None);
            }
        }
        await transaction.CommitAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> EditMenuButtonsAsync(string name, Guid userId, Guid roleId, Guid menuId, Guid tenantId, params IEnumerable<BMButtonModel> buttons)
    {
        var menu = await db.Menus.AsNoTrackingWithIdentityResolution()
               .FirstOrDefaultAsync(x => x.Id == menuId && x.TenantId == tenantId, RequestAborted);
        if (menu == null)
        {
            return false;
        }

        // 更新菜单名字
        await db.Menus
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == menuId &&
            !x.SoftDeleted
            && x.TenantId == tenantId
            )
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Name, name), CancellationToken.None);

        // 删除此角色全部菜单按钮重新添加
        var delQuery = from mbr in db.MenuButtonRoles
                       where mbr.RoleId == roleId &&
                       mbr.MenuId == menuId
                       && mbr.TenantId == tenantId
                       select mbr;
        await delQuery.ExecuteDeleteAsync(CancellationToken.None);
        await db.MenuButtonRoles.AddRangeAsync(buttons.Select(x => new BMMenuButtonRole
        {
            ButtonId = x.Id,
            MenuId = menuId,
            RoleId = roleId,
            TenantId = tenantId,
            ControllerName = GetControllerName(menu.Key, x.Type),
        }), CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> DeleteMenuButtonsAsync(Guid userId, Guid roleId, Guid menuId, Guid tenantId)
    {
        var menu = await db.Menus.AsNoTrackingWithIdentityResolution()
              .FirstOrDefaultAsync(x => x.Id == menuId && x.TenantId == tenantId, RequestAborted);
        if (menu == null)
        {
            return false;
        }
        // 删除此角色全部菜单按钮
        var delQuery = db.MenuButtonRoles.AsNoTrackingWithIdentityResolution()
            .Where(x => x.MenuId == menuId &&
            x.RoleId == roleId
            && x.TenantId == tenantId
            );
        await delQuery.ExecuteDeleteAsync(CancellationToken.None);
        // 如果是子菜单
        if (menu.ParentId.HasValue)
        {
            // 判断是否父菜单下有其他子菜单
            var isParentMenu = await (from r in db.Roles
                                      join bmbr in db.MenuButtonRoles on r.Id equals bmbr.RoleId
                                      join menuDB in db.Menus on bmbr.MenuId equals menuDB.Id
                                      where menuDB.ParentId == menu.ParentId &&
                                      r.Id == roleId
                                     && menuDB.TenantId == tenantId
                                      select bmbr).AnyAsync(CancellationToken.None);
            // 不存在子菜单删除父级菜单权限
            if (!isParentMenu)
            {
                await db.MenuButtonRoles
                      .AsNoTrackingWithIdentityResolution()
                      .Where(x =>
                      x.RoleId == roleId &&
                      x.MenuId == menu.ParentId
                      && x.TenantId == tenantId
                      )
                      .ExecuteDeleteAsync(CancellationToken.None);
            }
        }
        else
        {
            // 是否有子菜单
            var isParentMenu = await (from r in db.Roles
                                      join bmbr in db.MenuButtonRoles on r.Id equals bmbr.RoleId
                                      join menuDB in db.Menus on bmbr.MenuId equals menuDB.Id
                                      where menuDB.ParentId == menu.Id
                                      && menuDB.TenantId == tenantId &&
                                      r.Id == roleId
                                      select bmbr).AnyAsync(CancellationToken.None);
            // 删除全部子菜单权限
            if (isParentMenu)
            {
                await (from r in db.Roles
                       join bmbr in db.MenuButtonRoles on r.Id equals bmbr.RoleId
                       join menuDB in db.Menus on bmbr.MenuId equals menuDB.Id
                       where menuDB.ParentId == menu.Id
                       && menuDB.TenantId == tenantId &&
                       r.Id == roleId
                       select bmbr)
                      .ExecuteDeleteAsync(CancellationToken.None);
            }
        }
        return true;
    }

    public async Task<bool> DeleteMenuAsync(Guid menuId, Guid tenantId)
    {
        var menu = await db.Menus.AsNoTrackingWithIdentityResolution()
                    .Include(r => r.Children)
                    .FirstOrDefaultAsync(x => x.Id == menuId && x.TenantId == tenantId, RequestAborted);
        if (menu == null)
        {
            return false;
        }
        if (menu.Children?.Count > 0)
        {
            foreach (var item in menu.Children)
            {
                await DeleteMenuAsync(item.Id, tenantId);
            }
        }
        else
        {
            await db.MenuButtons
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x =>
                    x.MenuId == menuId
                    && x.TenantId == tenantId
                    )
                    .ExecuteDeleteAsync(CancellationToken.None);
            await db.MenuButtonRoles
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x =>
                    x.MenuId == menuId
                    && x.TenantId == tenantId
                    )
                    .ExecuteDeleteAsync(CancellationToken.None);
            await db.Menus
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x =>
                    x.Id == menuId
                    && x.TenantId == tenantId
                    )
                    .ExecuteDeleteAsync(CancellationToken.None);
        }
        return true;
    }

    public async Task<List<Guid>> GetRoleMenus(Guid userId, Guid? tenantId)
    {
        var role = await db.UserRoles.AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync(RequestAborted);
        if (role.Count > 0)
        {
            var query = db.MenuButtonRoles.AsNoTrackingWithIdentityResolution()
                    .Where(x => role.Contains(x.RoleId));
            if (tenantId.HasValue)
                query = query.Where(x => x.TenantId == tenantId);
            var r = await query.Select(x => x.MenuId).Distinct().ToListAsync(RequestAborted);
            return r;
        }
        else
        {
            return new();
        }
    }
}

file static class _
{
    internal static readonly Comparison<BMMenuTreeItem> menuComparisonBySort = (a, b) => (int)(a.Sort - b.Sort);

    internal static readonly Expression<Func<BMButton, BMButtonModel>> BtnExpr = it => new()
    {
        Id = it.Id,
        Name = it.Name,
        Type = it.Type,
        //Style = it.Style,
        Disable = it.Disable,
    };

    internal static readonly Expression<Func<BMMenu, BMMenuModel>> MenuExpr = item => new()
    {
        Id = item.Id,
        Name = item.Name,
        IconUrl = item.IconUrl,
        Key = item.Key,
        Sort = item.Sort,
        ParentId = item.ParentId,
        Url = item.Url,
        Note = item.Note,
    };
}