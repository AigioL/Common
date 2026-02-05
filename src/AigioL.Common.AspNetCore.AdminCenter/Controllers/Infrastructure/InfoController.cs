using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Services.Abstractions;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.JsonWebTokens.Services.Abstractions;
using AigioL.Common.Models;
using AntDesign;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

public static partial class InfoController
{
    /// <summary>
    /// 创建一个默认系统管理员账号，且在 DEBUG 下将返回 JsonWebToken，用于测试
    /// </summary>
    /// <param name="b"></param>
    /// <param name="pattern"></param>
    public static void MapPostInfo<TDbContext, TUser, TRole, TUserRole>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/info")
        where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>, IBMDbContextBase
        where TUser : BMUser, new()
        where TRole : BMRole, new()
        where TUserRole : BMUserRole
    {
        var routeGroup = b.MapGroup(pattern);

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] BMInitSystemRequest model,
            [FromQuery] bool onlyMigrate = false) =>
        {
            BMApiRsp<JsonWebTokenValue?> result;
            try
            {
                result = await PostAsync<TDbContext, TUser, TRole, TUserRole>(context, model, onlyMigrate);
            }
            catch (Exception ex)
            {
                result = new();
                result.SetException(ex);
            }
            return Results.Json(result, BMMinimalApisJsonSerializerContext.Default.BMApiRspJsonWebTokenValue);
        })
        .AllowAnonymous()
        .WithDescription("创建一个默认系统管理员账号");

        //        routeGroup.MapPost("migrate", async (HttpContext context) =>
        //        {
        //            BMApiRsp result;
        //            try
        //            {
        //                var db = context.RequestServices.GetRequiredService<TDbContext>();
        //#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        //                await db.Database.MigrateAsync(context.RequestAborted);
        //#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        //                result = true;
        //            }
        //            catch (Exception ex)
        //            {
        //                result = new();
        //                result.SetException(ex);
        //            }
        //            return Results.Json(result, BMMinimalApisJsonSerializerContext.Default.BMApiRsp);
        //        })
        //        .AllowAnonymous()
        //        .WithDescription("运行时应用迁移");
    }

    static async Task<BMApiRsp<JsonWebTokenValue?>> PostAsync<TDbContext, TUser, TRole, TUserRole>(
        HttpContext context,
        BMInitSystemRequest model,
        bool onlyMigrate)
        where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>, IBMDbContextBase
        where TUser : BMUser, new()
        where TRole : BMRole, new()
        where TUserRole : BMUserRole
    {
        BMApiRsp<JsonWebTokenValue?> result;

        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var db = context.RequestServices.GetRequiredService<TDbContext>();
        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var jwtValueProvider = context.RequestServices.GetRequiredService<IJsonWebTokenValueProvider>();
        var settings = context.RequestServices.GetRequiredService<IOptions<BMAppSettings>>().Value;

        Span<byte> hash = stackalloc byte[SHA384.HashSizeInBytes];
        SHA384.HashData(Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToString("yyyyMMdd") + settings.InitSystemSecuritySalt), hash);
        var hashPassword = Convert.ToHexString(hash);
        if (!string.Equals(hashPassword, model.InitPassword))
        {
            return result = HttpStatusCode.Unauthorized;
        }

        // https://learn.microsoft.com/zh-cn/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#apply-migrations-at-runtime
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        await db.Database.MigrateAsync(context.RequestAborted);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

        if (onlyMigrate)
        {
            return result = HttpStatusCode.OK;
        }

        if (!ShortGuid.TryParse(model.TenantId, out Guid tenantId))
        {
            result = "租户 Id 格式不正确";
        }
        if (string.IsNullOrWhiteSpace(model.TenantName))
        {
            return result = "租户名称不能为空或空白字符串";
        }

        var userName = model.UserName;
        if (string.IsNullOrWhiteSpace(userName) || userName == "string")
        {
            userName = settings.AdminUserName;
        }
        ArgumentNullException.ThrowIfNull(userName);

        // 幂等的添加或更新逻辑 👇
        using var transaction = db.Database.BeginTransaction();

        #region 添加管理员用户与预设角色

        var adminRoleName = adminCenterService.RoleNameAdministrator;
        List<string> addRoles = adminCenterService.AddRoles;
        var user = await userManager.FindByNameAsync(userName); // 查找默认初始管理员用户
        if (user == null)
        {
            user = new() // 创建默认初始管理员用户
            {
                UserName = userName,
                TenantId = tenantId,
            };
            var pwd = model.Password;
            if (string.IsNullOrWhiteSpace(pwd) || pwd == "string")
                pwd = settings.AdminPassword;
            ArgumentNullException.ThrowIfNull(pwd);
            var createResult = await userManager.CreateAsync(user, pwd);
            if (!createResult.Succeeded)
            {
                return result = createResult;
            }
            addRoles.Add(adminRoleName); // 添加管理员权限
        }
        else
        {
            var isUpdate = false; // 检查其他字段值是否需要更新
            if (user.TenantId != tenantId)
            {
                user.TenantId = tenantId;
                isUpdate = true;
            }
            if (isUpdate)
                await userManager.UpdateAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            if (!(roles != null && roles.Any(x => x == adminRoleName)))
            {
                // 更新角色
                addRoles.Add(adminRoleName);
            }
        }

        if (addRoles.Count != 0)
        {
            IdentityResult identityResult;
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
                        CreateUserId = user.Id,
                    };
                    db.Roles.Add(role);
                    await db.SaveChangesAsync();
                }

                if (roleName_ == adminRoleName)
                {
                    identityResult = await userManager.AddToRoleAsync(user, roleName_);
                    if (identityResult.Succeeded)
                    {
                        // 补上租户 Id
                        await db.UserRoles
                            .Where(x => x.UserId == user.Id && x.RoleId == role.Id)
                            .ExecuteUpdateAsync(x => x.SetProperty(y => y.TenantId, tenantId));
                        return result = identityResult;
                    }
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
                CreateUserId = user.Id,
                Name = model.TenantName,
            });
            await db.SaveChangesAsync();
        }

        #endregion

        var isRootTenant = tenantId == adminCenterService.RootTenantIdG;

        #region 添加预设菜单

        var queryDbMenus = from m in db.Menus.IgnoreAutoIncludes()
                           where m.TenantId == tenantId
                           select m;
        // 已有的菜单
        var dbMenus = await queryDbMenus.ToListAsync();

        // 要添加或更新的菜单
        var addMenus = new List<BMMenu>(GetBMMenus(isRootTenant));
        adminCenterService.HandleMenus(isRootTenant, addMenus);
        SetUserIdAndTenantId(addMenus, user.Id, tenantId);

        if (dbMenus.Count == 0)
        {
            await db.Menus.AddRangeAsync(addMenus);
            await db.SaveChangesAsync();
        }
        else
        {
            foreach (var menu in addMenus)
            {
                var dbMenu = dbMenus.FirstOrDefault(x => x.Key == menu.Key);
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
        var btnDict = new Dictionary<BMButtonType, string>
        {
            { BMButtonType.Edit, "编辑" },
            { BMButtonType.Delete, "删除" },
            { BMButtonType.Detail, "查看详情" },
            { BMButtonType.Add, "新增" },
            { BMButtonType.Query, "查询" },
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
                dbButton = new BMButton
                {
                    CreateUserId = user.Id,
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

        var expandBMMenus = new HashSet<BMMenu>();
        ExpandBMMenus(expandBMMenus, addMenus);
        foreach (var menu in expandBMMenus)
        {
            if (menu.Key == "dashboard" || (menu.Children != null && menu.Children.Count != 0))
            {
                var btnId = dbButtons.First(x => x.Type == BMButtonType.Query).Id;
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
                            await db.MenuButtonRoles.AddAsync(new BMMenuButtonRole
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
        return result = HttpStatusCode.OK;
    }

    static async Task TryAddAsync(Guid tenantId, Guid menuId, Guid btnId, DbSet<BMMenuButton> dbSet)
    {
        var any = await dbSet.AnyAsync(x => x.TenantId == tenantId && x.MenuId == menuId && x.ButtonId == btnId);
        if (!any)
        {
            await dbSet.AddAsync(new BMMenuButton
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
    static void ExpandBMMenus(HashSet<BMMenu> hashSet, params IEnumerable<BMMenu> menus)
    {
        foreach (var menu in menus)
        {
            hashSet.Add(menu);
            if (menu.Children != null && menu.Children.Count != 0)
            {
                ExpandBMMenus(hashSet, menu.Children);
            }
        }
    }

    internal static IEnumerable<BMMenu> GetBMMenus(bool isRootTenant)
    {
        yield return new BMMenu
        {
            Url = "/Statistics",
            Name = "统计分析",
            Key = "Statistics",
            IconUrl = IconType.Outline.AreaChart,
        };

        yield return new BMMenu
        {
            Url = "/Komaasharu",
            Name = "广告管理",
            Key = "Komaasharu",
            IconUrl = IconType.Outline.Crown,
            Children = [.. GetKomaasharuManage()],
        };

        yield return new BMMenu
        {
            Url = "/Identity",
            Name = "用户管理",
            Key = "Identity",
            IconUrl = IconType.Outline.User,
            Children = [.. GetUserManage()],
        };

        yield return new BMMenu
        {
            Url = "/Basics",
            Name = "通用管理",
            Key = "Basics",
            IconUrl = IconType.Outline.Profile,
            Children = [.. GetBasicsManage()],
        };

        yield return new BMMenu
        {
            Url = "/Ordering",
            Name = "订单管理",
            Key = "Ordering",
            IconUrl = IconType.Outline.ShoppingCart,
            Children = [.. GetOrderingManage()],
        };

        yield return new BMMenu
        {
            Url = "/Role",
            Name = "角色管理",
            Key = "RoleManageMenu",
            IconUrl = IconType.Outline.UserSwitch,
            Children = [.. GetRoleManage()],
        };

        yield return new BMMenu
        {
            Url = "/SystemManage",
            Name = "系统管理",
            Key = "SystemManage",
            IconUrl = IconType.Outline.Control,
            Children = [.. GetSystemManage()],
        };

        IEnumerable<BMMenu> GetKomaasharuManage()
        {
            yield return new BMMenu
            {
                Url = "/Komaasharu/Manage",
                Name = "广告管理",
                Key = ControllerConstants.AdvertisementManage,
                IconUrl = IconType.Outline.Crown,
            };
        }

        IEnumerable<BMMenu> GetUserManage()
        {
            yield return new BMMenu
            {
                Url = "/Identity/UserList",
                Name = "用户列表",
                Key = ControllerConstants.ClientUser,
                IconUrl = IconType.Outline.User,
            };

            yield return new BMMenu
            {
                Url = "/Identity/UserDeviceList",
                Name = "用户设备列表",
                Key = ControllerConstants.UserDevice,
                IconUrl = IconType.Outline.Mobile,
            };

            yield return new BMMenu
            {
                Url = "/Identity/ExternalAccountList",
                Name = "外部账号列表",
                Key = ControllerConstants.ExternalAccount,
                IconUrl = IconType.Outline.Twitter,
            };

            yield return new BMMenu
            {
                Url = "/Identity/UserCancelList",
                Name = "注销用户列表",
                Key = ControllerConstants.UserCancel,
                IconUrl = IconType.Outline.UserDelete,
            };

            yield return new BMMenu
            {
                Url = "/Identity/AuthMessageRecord",
                Name = "短信记录查询",
                Key = ControllerConstants.AuthMessageRecord,
                IconUrl = IconType.Outline.Send,
            };
        }

        IEnumerable<BMMenu> GetBasicsManage()
        {
            yield return new BMMenu
            {
                Url = "/Basics/KeyValuePair",
                Name = "键值对管理",
                Key = ControllerConstants.KeyValuePair,
                IconUrl = IconType.Outline.Profile,
            };

            yield return new BMMenu
            {
                Url = "/Basics/StaticResource",
                Name = "静态资源与记录管理",
                Key = ControllerConstants.StaticResource,
                IconUrl = IconType.Outline.FileImage,
            };

            yield return new BMMenu
            {
                Url = "/Basics/Article",
                Name = "文章管理",
                Key = ControllerConstants.Article,
                IconUrl = IconType.Outline.FilePdf,
            };

            yield return new BMMenu
            {
                Url = "/Basics/ArticleTag",
                Name = "文章标签管理",
                Key = ControllerConstants.ArticleTag,
                IconUrl = IconType.Outline.Tags,
            };

            yield return new BMMenu
            {
                Url = "/Basics/ArticleCategory",
                Name = "文章分类管理",
                Key = ControllerConstants.ArticleCategory,
                IconUrl = IconType.Outline.Group,
            };

            yield return new BMMenu
            {
                Url = "/Basics/WebProxys",
                Name = "网络代理管理",
                Key = ControllerConstants.WebProxys,
                IconUrl = IconType.Outline.Ie,
            };
        }

        IEnumerable<BMMenu> GetOrderingManage()
        {
            yield return new BMMenu
            {
                Url = "/Ordering/OrderManage",
                Name = "订单列表",
                Key = ControllerConstants.Order,
                IconUrl = IconType.Outline.ShoppingCart,
            };

            yield return new BMMenu
            {
                Url = "/Ordering/AftersalesBillManage",
                Name = "售后列表",
                Key = ControllerConstants.AftersalesBill,
                IconUrl = IconType.Outline.Backward,
            };

            yield return new BMMenu
            {
                Url = "/Ordering/RefundBillManage",
                Name = "退款列表",
                Key = ControllerConstants.RefundBill,
                IconUrl = IconType.Outline.Rollback,
            };

            yield return new BMMenu
            {
                Url = "/Ordering/MerchantDeductionAgreementConfigurationManage",
                Name = "扣款协议配置",
                Key = ControllerConstants.MerchantDeductionAgreementConfiguration,
                IconUrl = IconType.Outline.PayCircle,
            };

            yield return new BMMenu
            {
                Url = "/Ordering/OrderBusinessPaymentConfigurationManage",
                Name = "业务类型支付配置",
                Key = ControllerConstants.OrderBusinessPaymentConfiguration,
                IconUrl = IconType.Outline.Alipay,
            };
        }

        IEnumerable<BMMenu> GetSystemManage()
        {
            yield return new BMMenu
            {
                Url = "/System/User",
                Name = "后台用户",
                Key = ControllerConstants.SystemUser,
                IconUrl = IconType.Outline.User,
            };

            yield return new BMMenu
            {
                Url = "/System/MenuManage",
                Name = "系统菜单管理",
                Key = ControllerConstants.SystemMenuManage,
                IconUrl = IconType.Outline.Menu,
            };

            yield return new BMMenu
            {
                Url = "/System/Info",
                Name = "系统信息",
                Key = "SystemInfo",
                IconUrl = IconType.Outline.Info,
            };

            //if (isRootTenant)
            //{
            //    yield return new BMMenu
            //    {
            //        Url = "/" + nameof(Titles.tenants),
            //        Name = Titles.tenants,
            //        Key = nameof(Titles.tenants),
            //        //Icon = IconType.Outline.Group,
            //    };

            //    yield return new BMMenu
            //    {
            //        Url = "/" + nameof(Titles.tenantproductkeybalances),
            //        Name = Titles.tenantproductkeybalances,
            //        Key = nameof(Titles.tenantproductkeybalances),
            //        //Icon = IconType.Outline.Tags,
            //    };

            //    yield return new BMMenu
            //    {
            //        Url = "/" + nameof(Titles.tenantproductkeybalancerecords),
            //        Name = Titles.tenantproductkeybalancerecords,
            //        Key = nameof(Titles.tenantproductkeybalancerecords),
            //        //Icon = IconType.Outline.UnorderedList,
            //    };
            //}
        }

        IEnumerable<BMMenu> GetRoleManage()
        {
            yield return new BMMenu
            {
                Url = "/Role/Manage",
                Name = "角色管理",
                Key = ControllerConstants.RoleManage,
                IconUrl = IconType.Outline.UserSwitch,
            };

            yield return new BMMenu
            {
                Url = "/Role/Menu",
                Name = "角色菜单管理",
                Key = "RoleMenu",
                IconUrl = IconType.Outline.Menu,
            };
        }
    }

    internal static void SetUserIdAndTenantId(IEnumerable<BMMenu>? menus, Guid userId, Guid tenantId, long? sort = null)
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
            menu.TenantId = tenantId;
            menu.Sort = sort.Value;
            sort -= 10;
            SetUserIdAndTenantId(menu.Children, userId, tenantId, sort);
        }
    }
}