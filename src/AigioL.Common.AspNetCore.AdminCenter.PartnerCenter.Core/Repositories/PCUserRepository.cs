using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GameTrainer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Repositories;

sealed partial class PCUserRepository<TDbContext>(
    IIdentityUserManager<PCUser> userManager,
    TDbContext dbContext,
    IServiceProvider serviceProvider) :
    Repository<TDbContext, PCUser, Guid>(dbContext, serviceProvider),
    IPCUserRepository
    where TDbContext : DbContext, IIdentityDbContext<PCUser, PCRole, Guid, PCUserClaim, PCUserRole, PCUserLogin, PCRoleClaim, PCUserToken>, IPCDbContext2
{
    const string UserNotFoundMessage = "KOL 用户不存在";
    const string PhoneNumberDuplicatedMessage = "手机号已存在，请更换后重试";
    const string PhoneNumberRequiredMessage = "手机号不能为空";

    public UserManager<PCUser> UserManager => userManager.Impl;

    public async Task<PagedModel<PCUserTableItem>> QueryAsync(
        Guid? id,
        PCUserType? userType,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        Guid? businessId,
        bool? disable,
        DateTimeOffset?[]? createTime,
        DateTimeOffset?[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<PCUser> query = db.Users.AsNoTrackingWithIdentityResolution();

        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        if (userType.HasValue)
            query = query.Where(x => x.UserType == userType.Value);
        if (!string.IsNullOrWhiteSpace(phoneNumber))
            query = query.Where(x => x.PhoneNumber!.Contains(phoneNumber));
        if (!string.IsNullOrWhiteSpace(phoneNumberRegionCode))
            query = query.Where(x => x.PhoneNumberRegionCode == phoneNumberRegionCode);
        if (businessId.HasValue)
            query = query.Where(x => x.BusinessIds.Contains(businessId.Value));
        if (disable.HasValue)
            query = query.Where(x => x.Disable == disable.Value);
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }
        if (updateTime != null && updateTime.Length == 2)
        {
            if (updateTime[0].HasValue)
                query = query.Where(x => x.UpdateTime >= updateTime[0]);
            if (updateTime[1].HasValue)
                query = query.Where(x => x.UpdateTime < updateTime[1]);
        }
        if (!string.IsNullOrWhiteSpace(createUser))
            if (ShortGuid.TryParse(createUser, out Guid createUserId))
                query = query.Where(x => x.CreateUser!.Id == createUserId);
            else
                query = query.Where(x => x.CreateUser!.NickName!.Contains(createUser));
        if (!string.IsNullOrWhiteSpace(operatorUser))
            if (ShortGuid.TryParse(operatorUser, out Guid operatorUserId))
                query = query.Where(x => x.OperatorUser!.Id == operatorUserId);
            else
                query = query.Where(x => x.OperatorUser!.NickName!.Contains(operatorUser));

        query = !string.IsNullOrWhiteSpace(orderBy)
            ? query.OrderByPropertyName(orderBy, desc)
            : query.OrderByDescending(x => x.CreateTime);

        return await query.ProjectTo<PCUserTableItem>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
    }

    public async Task<ApiRsp<IdentityResult?>> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditPCUserModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = await userManager.FindByIdAsync(model.Id);
        if (entity == null)
        {
            return UserNotFoundMessage;
        }

        var phoneNumber = NormalizePhoneNumber(model.PhoneNumber);
        var phoneNumberRegionCode = model.PhoneNumberRegionCode ?? IPhoneNumber.DefaultPhoneNumberRegionCode;

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return PhoneNumberRequiredMessage;
        }

        if (await ExistsByPhoneNumberAsync(phoneNumber, phoneNumberRegionCode, model.Id, cancellationToken))
        {
            return PhoneNumberDuplicatedMessage;
        }

        mapper.Map(model, entity);
        entity.PhoneNumber = phoneNumber;
        entity.PhoneNumberRegionCode = phoneNumberRegionCode;
        entity.BusinessIds = NormalizeBusinessIds(model.BusinessIds);
        entity.OperatorUserId = operatorUserId;

        var identityResult = await userManager.UpdateAsync(entity);
        return identityResult;
    }

    public async Task<ApiRsp<IdentityResult?>> InsertAsync(
        AddOrEditPCUserModel model,
        List<PCMenu> addMenus,
        bool isRootTenant,
        Guid tenantId,
        string? tenantName,
        Guid? userId,
        Guid? pcUserId,
        string adminRoleName,
        HashSet<string>? addRoles = null,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var phoneNumber = NormalizePhoneNumber(model.PhoneNumber);
        var phoneNumberRegionCode = model.PhoneNumberRegionCode ?? IPhoneNumber.DefaultPhoneNumberRegionCode;

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return PhoneNumberRequiredMessage;
        }

        if (await ExistsByPhoneNumberAsync(phoneNumber, phoneNumberRegionCode, null, cancellationToken))
        {
            return PhoneNumberDuplicatedMessage;
        }

        var entity = mapper.Map<PCUser>(model);
        entity.Id = default;
        entity.PhoneNumber = phoneNumber;
        entity.PhoneNumberRegionCode = phoneNumberRegionCode;
        entity.UserName = $"{phoneNumberRegionCode}{phoneNumber}";
        entity.PhoneNumberConfirmed = true;
        entity.BusinessIds = NormalizeBusinessIds(model.BusinessIds);
        entity.CreateUserId = userId;
        entity.CreatePCUserId = pcUserId;
        entity.TenantId = tenantId;

        await InitSysAsync(addMenus, isRootTenant, tenantId, tenantName, userId, pcUserId, adminRoleName, addRoles);

        var identityResult = await userManager.Impl.CreateAsync(entity);
        if (identityResult.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(adminRoleName))
            {
                if (entity.Id != default)
                {
                    // 补上角色
                    identityResult = await userManager.Impl.AddToRoleAsync(entity, adminRoleName);
                    if (identityResult.Succeeded)
                    {
                        // 补上租户 Id
                        await userManager.UpdateTenantIdAsync(entity.Id, tenantId);
                    }
                }
            }
        }
        return identityResult;
    }

    public async Task<int> SwitchAsync(
        Guid primaryKey,
        Guid? operatorUserId,
        bool? disable,
        CancellationToken cancellationToken = default)
    {
        var query = db.Users
            .IgnoreQueryFilters()
            .Where(x => x.Id == primaryKey);

        if (disable.HasValue)
        {
            return await query.ExecuteUpdateAsync(x => x
                .SetProperty(y => y.Disable, y => disable.Value)
                .SetProperty(y => y.OperatorUserId, y => operatorUserId)
                , cancellationToken);
        }

        return await query.ExecuteUpdateAsync(x => x
            .SetProperty(y => y.Disable, y => !y.Disable)
            .SetProperty(y => y.OperatorUserId, y => operatorUserId)
            , cancellationToken);
    }

    async Task<bool> ExistsByPhoneNumberAsync(
        string phoneNumber,
        string? phoneNumberRegionCode,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        IQueryable<PCUser> query = db.Users.AsNoTrackingWithIdentityResolution();
        if (currentId.HasValue)
        {
            query = query.Where(x => x.Id != currentId.Value);
        }

        return await query.AnyAsync(x => x.PhoneNumber == phoneNumber && x.PhoneNumberRegionCode == phoneNumberRegionCode, cancellationToken);
    }

    async Task<PCUser?> FindByPhoneNumberAsync(
        string phoneNumber,
        string? phoneNumberRegionCode,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        IQueryable<PCUser> query = db.Users;
        if (currentId.HasValue)
        {
            query = query.Where(x => x.Id != currentId.Value);
        }

        return await query.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber && x.PhoneNumberRegionCode == phoneNumberRegionCode, cancellationToken);
    }

    static Guid[] NormalizeBusinessIds(Guid[]? businessIds) => businessIds?.Distinct().ToArray() ?? [];

    static string NormalizePhoneNumber(string? phoneNumber) => phoneNumber?.Trim() ?? string.Empty;
}

partial class PCUserRepository<TDbContext>
{
    public async Task<PCUserOrderFilterModel> BuildOrderFilterAsync(
        PCUser user,
        CancellationToken cancellationToken = default)
    {
        var businessIds = user.BusinessIds
            .Where(static x => x != default)
            .Distinct()
            .ToArray();

        if (businessIds.Length == 0)
        {
            return new PCUserOrderFilterModel
            {
                UserType = user.UserType,
                BusinessIds = [],
            };
        }

        if (user.UserType == PCUserType.Channel)
        {
            return new PCUserOrderFilterModel
            {
                UserType = PCUserType.Channel,
                BusinessIds = businessIds,
            };
        }

        if (user.UserType == PCUserType.PromoCode)
        {
            return new PCUserOrderFilterModel
            {
                UserType = PCUserType.PromoCode,
                BusinessIds = await db.Set<PromoCode>()
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x => businessIds.Contains(x.Id) && x.RevenueShareRecipientKolUserId.HasValue)
                    .Select(x => x.RevenueShareRecipientKolUserId!.Value)
                    .Distinct()
                    .ToArrayAsync(cancellationToken),
            };
        }

        return new PCUserOrderFilterModel
        {
            UserType = user.UserType,
            BusinessIds = businessIds,
        };
    }
}

partial class PCUserRepository<TDbContext> // Init 初始化 PC 后台的权限与菜单
{
    public async Task InitSysAsync(
        List<PCMenu> addMenus,
        bool isRootTenant,
        Guid tenantId,
        string? tenantName,
        Guid? userId,
        Guid? pcUserId,
        string adminRoleName,
        HashSet<string>? addRoles = null)
    {
        // 幂等的添加或更新逻辑 👇
        using var transaction = db.Database.BeginTransaction();

        #region 添加预设角色

        addRoles ??= new();
        addRoles.Add(adminRoleName);

        if (addRoles.Count != 0)
        {
            foreach (var roleName_ in addRoles)
            {
                var role = await db.Roles.FirstOrDefaultAsync(x => x.Name == roleName_ && x.TenantId == tenantId);
                if (role == null)
                {
                    role = new()
                    {
                        Name = roleName_,
                        NormalizedName = roleName_.ToUpper(),
                        TenantId = tenantId,
                        CreateUserId = userId,
                    };
                    db.Roles.Add(role);
                    await db.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region 添加租户

        var tenant = await db.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            await db.Tenants.AddAsync(tenant = new()
            {
                Id = tenantId,
                CreateUserId = userId,
                Name = tenantName ?? "",
            });
            await db.SaveChangesAsync();
        }

        #endregion

        #region 添加预设菜单

        var queryDbMenus = from m in db.Menus.IgnoreAutoIncludes()
                           where m.TenantId == tenantId
                           select m;
        // 已有的菜单
        var dbMenus = await queryDbMenus.ToListAsync();

        // 要添加或更新的菜单
        SetUserIdAndTenantId(addMenus, userId, pcUserId, tenantId);

        var expandMenus = new HashSet<PCMenu>();
        ExpandPCMenus(expandMenus, addMenus);

        var expandDbMenus = new HashSet<PCMenu>();
        ExpandPCMenus(expandDbMenus, dbMenus);

        if (dbMenus.Count == 0)
        {
            await db.Menus.AddRangeAsync(addMenus);
            await db.SaveChangesAsync();
        }
        else
        {
            foreach (var menu in expandMenus)
            {
                var dbMenu = expandDbMenus.FirstOrDefault(x => x.Key == menu.Key);
                if (dbMenu == null)
                {
                    // 添加新菜单
                    await db.Menus.AddRangeAsync(menu);
                }
                else
                {
                    // 更新数据库实体
                    menu.Id = dbMenu.Id;
                    dbMenu.Url = menu.Url;
                    dbMenu.Name = menu.Name;
                    dbMenu.IconUrl = menu.IconUrl;
                }
            }
            await db.SaveChangesAsync();
        }

        #endregion

        #region 添加预设按钮

        var queryDbButtons = from b in db.Buttons.IgnoreAutoIncludes()
                             where b.TenantId == tenantId
                             select b;
        // 已有的按钮
        var dbButtons = await queryDbButtons.ToListAsync();

        // 预设的按钮组
        var btnDict = new Dictionary<PCButtonType, string>
        {
            { PCButtonType.Edit, "编辑" },
            { PCButtonType.Delete, "删除" },
            { PCButtonType.Detail, "查看详情" },
            { PCButtonType.Add, "新增" },
            { PCButtonType.Query, "查询" },
        };
        foreach (var it in btnDict)
        {
            var dbButton = dbButtons.FirstOrDefault(x => x.Type == it.Key);
            if (dbButton != null)
            {
                dbButton.Name = it.Value;
            }
            else
            {
                dbButton = new PCButton
                {
                    CreateUserId = userId,
                    TenantId = tenantId,
                    Name = it.Value,
                    Type = it.Key,
                };
                dbButtons.Add(dbButton);
                await db.Buttons.AddAsync(dbButton);
            }
        }
        await db.SaveChangesAsync();

        #endregion

        #region 添加预设菜单按钮关系

        foreach (var menu in expandMenus)
        {
            if (menu.Key == "dashboard" || (menu.Children != null && menu.Children.Count != 0))
            {
                var btnId = dbButtons.First(x => x.Type == PCButtonType.Query).Id;
                await TryAddAsync(tenantId, menu.Id, btnId, db.MenuButtons);
                continue;
            }

            foreach (var btn in dbButtons)
            {
                await TryAddAsync(tenantId, menu.Id, btn.Id, db.MenuButtons);
            }
        }
        await db.SaveChangesAsync();

        #endregion

        #region 添加预设菜单按钮角色关系

        // 仅处理管理员角色
        var adminRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == adminRoleName && x.TenantId == tenantId);
        if (adminRole != null)
        {
            var adminRoleMenus = await db.Menus
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Buttons)
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
            foreach (var menu in adminRoleMenus)
            {
                var btns = menu.Buttons;
                if (btns != null)
                {
                    foreach (var btn in btns)
                    {
                        var controllerName = menu.Key + btn.Type;
                        // 先查询是否已存在
                        var any = await db.MenuButtonRoles.AnyAsync(x =>
                            x.TenantId == tenantId &&
                            x.RoleId == adminRole.Id &&
                            x.MenuId == menu.Id &&
                            x.ButtonId == btn.Id &&
                            x.ControllerName == controllerName);
                        if (!any)
                        {
                            await db.MenuButtonRoles.AddAsync(new PCMenuButtonRole
                            {
                                TenantId = tenantId,
                                RoleId = adminRole.Id,
                                MenuId = menu.Id,
                                ButtonId = btn.Id,
                                ControllerName = controllerName,
                            });
                        }
                    }
                }
            }
            await db.SaveChangesAsync();
        }

        #endregion

        await transaction.CommitAsync();
    }

    static async Task TryAddAsync(Guid tenantId, Guid menuId, Guid btnId, DbSet<PCMenuButton> dbSet)
    {
        var any = await dbSet.AnyAsync(x => x.TenantId == tenantId && x.MenuId == menuId && x.ButtonId == btnId);
        if (!any)
        {
            await dbSet.AddAsync(new PCMenuButton
            {
                TenantId = tenantId,
                MenuId = menuId,
                ButtonId = btnId,
            });
        }
    }

    /// <summary>
    /// 将树状菜单展开为一级集合
    /// </summary>
    static void ExpandPCMenus(HashSet<PCMenu> hashSet, params IEnumerable<PCMenu> menus)
    {
        foreach (var menu in menus)
        {
            hashSet.Add(menu);
            if (menu.Children != null && menu.Children.Count != 0)
            {
                ExpandPCMenus(hashSet, menu.Children);
            }
        }
    }

    static void SetUserIdAndTenantId(IEnumerable<PCMenu>? menus, Guid? userId, Guid? pcUserId, Guid tenantId, long? sort = null)
    {
        if (menus == null)
        {
            return;
        }
        if (!sort.HasValue)
        {
            // 从大的数开始递减，保证插入数据库时排序正确
            sort = 10_0000L;
        }
        foreach (var menu in menus)
        {
            menu.CreateUserId = userId;
            menu.CreatePCUserId = pcUserId;
            menu.TenantId = tenantId;
            menu.Sort = sort.Value;
            sort -= 10;
            SetUserIdAndTenantId(menu.Children, userId, pcUserId, tenantId, sort);
        }
    }
}