using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.JsonWebTokens.Services.Abstractions;
using AigioL.Common.Models;
using AntDesign;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

public partial class InfoController
{
    public const string RoleNameAdministrator = "Administrator";
}

public partial class InfoController<TDbContext, TUser, TRole, TUserRole, TRoleEnum> : InfoController
    where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>, IBMDbContextBase
    where TUser : BMUser, new()
    where TRole : BMRole, new()
    where TUserRole : BMUserRole
    where TRoleEnum : struct, Enum
{
    /// <summary>
    /// 创建一个默认系统管理员账号，且在 DEBUG 下将返回 JsonWebToken，用于测试
    /// </summary>
    /// <param name="b"></param>
    /// <param name="pattern"></param>
    public void MapPostInfo(
        IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/info")
    {
        var routeGroup = b.MapGroup(pattern);

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] BMInitSystemRequest model,
            [FromQuery] bool onlyMigrate) =>
        {
            BMApiRsp<JsonWebTokenValue> result;
            try
            {
                result = await PostAsync(context, model, onlyMigrate);
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

    protected new virtual string RoleNameAdministrator => InfoController.RoleNameAdministrator;

    protected virtual Guid RootTenantIdG => TenantConstants.RootTenantIdG;

    protected async Task<BMApiRsp<JsonWebTokenValue>> PostAsync(
        HttpContext context,
        BMInitSystemRequest model,
        bool onlyMigrate)
    {
        BMApiRsp<JsonWebTokenValue> result;

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

        using var transaction = db.Database.BeginTransaction();

        #region 添加管理员用户与预设角色

        var adminRoleName = RoleNameAdministrator;
        List<string> addRoles = [.. Enum.GetValues<TRoleEnum>().Select(x => x.ToString()).Where(x => x != RoleNameAdministrator)];
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
                    if (!identityResult.Succeeded)
                    {
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
            await db.Tenants.AddAsync(new()
            {
                Id = tenantId,
                CreateUserId = user.Id,
                Name = model.TenantName,
            });
            await db.SaveChangesAsync();
        }

        #endregion

        var isRootTenant = tenantId == RootTenantIdG;

        #region 添加预设菜单

        var menus = await db.Menus.Where(x => x.TenantId == tenantId).ToListAsync();
        if (menus.Count == 0)
        {
            menus = await AddMenusAsync();
        }

        async Task<List<BMMenu>> AddMenusAsync()
        {
            var menus = new List<BMMenu>(GetBMMenus(isRootTenant));
            SetUserIdAndTenantId(menus, user.Id, tenantId);

            await db.Menus.AddRangeAsync(menus);
            await db.SaveChangesAsync();
            return menus;
        }

        #endregion

        #region 添加预设按钮

        var btns = await db.Buttons.Where(x => x.TenantId == tenantId).ToListAsync();
        if (btns.Count == 0) btns = await AddButtonsAsync();

        async Task<List<BMButton>> AddButtonsAsync()
        {
            var btnDict = new Dictionary<BMButtonType, string>
            {
                { BMButtonType.Edit, "编辑" },
                { BMButtonType.Delete, "删除" },
                { BMButtonType.Detail, "查看详情" },
                { BMButtonType.Add, "新增" },
                { BMButtonType.Query, "查询" },
            };
            var btns = btnDict.Select(x => new BMButton
            {
                CreateUserId = user.Id,
                TenantId = user.TenantId,
                Name = x.Value,
                Type = x.Key,
            }).ToList();

            await db.Buttons.AddRangeAsync(btns);
            await db.SaveChangesAsync();
            return btns;
        }

        #endregion

        #region 添加预设菜单按钮关系

        var menuButtons = await db.MenuButtons.Where(x => x.TenantId == tenantId).ToListAsync();
        if (menuButtons.Count == 0) menuButtons = await AddMenuButtonsAsync();

        async Task<List<BMMenuButton>> AddMenuButtonsAsync()
        {
            var menuButtons = new List<BMMenuButton>();

            foreach (var menu in menus.Concat(menus
                .Where(x => x.Children != null)
                .SelectMany(x => x.Children!)))
            {
                if (menu.Key == "dashboard" ||
                    (menu.Children != null && menu.Children.Count != 0))
                {
                    menuButtons.Add(new BMMenuButton
                    {
                        TenantId = tenantId,
                        MenuId = menu.Id,
                        ButtonId = btns.First(x => x.Type == BMButtonType.Query).Id,
                    });
                    continue;
                }

                foreach (var btn in btns)
                {
                    menuButtons.Add(new BMMenuButton
                    {
                        TenantId = tenantId,
                        MenuId = menu.Id,
                        ButtonId = btn.Id,
                    });
                }
            }

            await db.MenuButtons.AddRangeAsync(menuButtons);
            await db.SaveChangesAsync();
            return menuButtons;
        }

        #endregion

        #region 添加预设菜单按钮角色关系

        var menuButtonRoles = await db.MenuButtonRoles.Where(x => x.TenantId == tenantId).ToListAsync();
        if (menuButtonRoles.Count == 0) await AddMenuButtonRolesAsync();

        async Task AddMenuButtonRolesAsync()
        {
            if (addRoles.Count != 0)
            {
                // 添加 系统管理员 权限
                var adminRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == "Administrator" && x.TenantId == tenantId);
                if (adminRole != null)
                {
                    var adminRoleMenus = await db.Menus.Include(x => x.Buttons).Where(x => x.TenantId == tenantId)
                        .AsNoTrackingWithIdentityResolution().ToListAsync();

                    foreach (var menu in adminRoleMenus)
                        foreach (var btn in menu.Buttons!)
                            await db.MenuButtonRoles.AddAsync(new BMMenuButtonRole
                            {
                                TenantId = tenantId,
                                RoleId = adminRole.Id,
                                MenuId = menu.Id,
                                ButtonId = btn.Id,
                                ControllerName = menu.Key + btn.Type,
                            });

                    if (isRootTenant)
                    {
                    }
                }

                await db.SaveChangesAsync();
            }
        }

        #endregion

        await transaction.CommitAsync();
        return result = HttpStatusCode.OK;
    }

    static Guid GetGuid(int seed)
    {
        Span<char> chars = stackalloc char[32];
        chars.Fill('0');
        var seedStr = seed.ToString();

        var index = chars.Length - 1;
        for (int i = seedStr.Length - 1; i >= 0; i--)
        {
            chars[index--] = seedStr[i];
        }

        return Guid.ParseExact(chars, "N");
    }

    protected virtual IEnumerable<BMMenu> GetBMMenus(bool isRootTenant)
    {
        int seed = 1000;
        yield return new BMMenu
        {
            //Id = GetGuid(seed),
            Url = "/Statistics",
            Name = "统计分析",
            Key = "Statistics",
            IconUrl = IconType.Outline.AreaChart,
        };

        yield return new BMMenu
        {
            //Id = GetGuid(seed += 100),
            Url = "/Komaasharu",
            Name = "广告管理",
            Key = "Komaasharu",
            IconUrl = IconType.Outline.Crown,
            Children = [.. GetKomaasharuManage(seed)],
        };

        yield return new BMMenu
        {
            //Id = GetGuid(seed += 100),
            Url = "/Identity",
            Name = "用户管理",
            Key = "Identity",
            IconUrl = IconType.Outline.User,
            Children = [.. GetUserManage(seed)],
        };

        yield return new BMMenu
        {
            //Id = GetGuid(seed += 100),
            Url = "/Basics",
            Name = "通用管理",
            Key = "Basics",
            IconUrl = IconType.Outline.Profile,
            Children = [.. GetBasicsManage(seed)],
        };

        yield return new BMMenu
        {
            //Id = GetGuid(seed += 100),
            Url = "/Ordering",
            Name = "订单管理",
            Key = "Ordering",
            IconUrl = IconType.Outline.ShoppingCart,
            Children = [.. GetOrderingManage(seed)],
        };

        yield return new BMMenu
        {
            //Id = GetGuid(seed += 100),
            Url = "/Role",
            Name = "角色管理",
            Key = "RoleManageMenu",
            IconUrl = IconType.Outline.UserSwitch,
            Children = [.. GetRoleManage(seed)],
        };

        yield return new BMMenu
        {
            //Id = GetGuid(seed += 100),
            Url = "/SystemManage",
            Name = "系统管理",
            Key = "SystemManage",
            IconUrl = IconType.Outline.Control,
            Children = [.. GetSystemManage(seed)],
        };

        IEnumerable<BMMenu> GetKomaasharuManage(int seed)
        {
            yield return new BMMenu
            {
                //Id = GetGuid(seed + 5),
                Url = "/Komaasharu/Manage",
                Name = "广告管理",
                Key = ControllerConstants.AdvertisementManage,
                IconUrl = IconType.Outline.Crown,
            };
        }

        IEnumerable<BMMenu> GetUserManage(int seed)
        {
            yield return new BMMenu
            {
                //Id = GetGuid(seed + 5),
                Url = "/Identity/UserList",
                Name = "用户列表",
                Key = ControllerConstants.ClientUser,
                IconUrl = IconType.Outline.User,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 10),
                Url = "/Identity/UserDeviceList",
                Name = "用户设备列表",
                Key = ControllerConstants.UserDevice,
                IconUrl = IconType.Outline.Mobile,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 15),
                Url = "/Identity/ExternalAccountList",
                Name = "外部账号列表",
                Key = ControllerConstants.ExternalAccount,
                IconUrl = IconType.Outline.Twitter,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 20),
                Url = "/Identity/UserCancelList",
                Name = "注销用户列表",
                Key = ControllerConstants.UserCancel,
                IconUrl = IconType.Outline.UserDelete,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 25),
                Url = "/Identity/AuthMessageRecord",
                Name = "短信记录查询",
                Key = ControllerConstants.AuthMessageRecord,
                IconUrl = IconType.Outline.Send,
            };
        }

        IEnumerable<BMMenu> GetBasicsManage(int seed)
        {
            yield return new BMMenu
            {
                //Id = GetGuid(seed + 5),
                Url = "/Basics/KeyValuePair",
                Name = "键值对管理",
                Key = ControllerConstants.KeyValuePair,
                IconUrl = IconType.Outline.Profile,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 10),
                Url = "/Basics/StaticResource",
                Name = "静态资源与记录管理",
                Key = ControllerConstants.StaticResource,
                IconUrl = IconType.Outline.FileImage,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 15),
                Url = "/Basics/Article",
                Name = "文章管理",
                Key = ControllerConstants.Article,
                IconUrl = IconType.Outline.FilePdf,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 20),
                Url = "/Basics/ArticleTag",
                Name = "文章标签管理",
                Key = ControllerConstants.ArticleTag,
                IconUrl = IconType.Outline.Tags,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 25),
                Url = "/Basics/ArticleCategory",
                Name = "文章分类管理",
                Key = ControllerConstants.ArticleCategory,
                IconUrl = IconType.Outline.Group,
            };
        }

        IEnumerable<BMMenu> GetOrderingManage(int seed)
        {
            yield return new BMMenu
            {
                //Id = GetGuid(seed + 5),
                Url = "/Ordering/OrderManage",
                Name = "订单列表",
                Key = ControllerConstants.Order,
                IconUrl = IconType.Outline.ShoppingCart,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 10),
                Url = "/Ordering/AftersalesBillManage",
                Name = "售后列表",
                Key = ControllerConstants.AftersalesBill,
                IconUrl = IconType.Outline.Backward,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 15),
                Url = "/Ordering/RefundBillManage",
                Name = "退款列表",
                Key = ControllerConstants.RefundBill,
                IconUrl = IconType.Outline.Rollback,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 25),
                Url = "/Ordering/MerchantDeductionAgreementConfigurationManage",
                Name = "扣款协议配置",
                Key = ControllerConstants.MerchantDeductionAgreementConfiguration,
                IconUrl = IconType.Outline.PayCircle,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 25),
                Url = "/Ordering/OrderBusinessPaymentConfigurationManage",
                Name = "业务类型支付配置",
                Key = ControllerConstants.OrderBusinessPaymentConfiguration,
                IconUrl = IconType.Outline.Alipay,
            };
        }

        IEnumerable<BMMenu> GetSystemManage(int seed)
        {
            yield return new BMMenu
            {
                //Id = GetGuid(seed + 5),
                Url = "/System/User",
                Name = "后台用户",
                Key = ControllerConstants.SystemUser,
                IconUrl = IconType.Outline.User,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 10),
                Url = "/System/MenuManage",
                Name = "系统菜单管理",
                Key = ControllerConstants.SystemMenuManage,
                IconUrl = IconType.Outline.Menu,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 15),
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

        IEnumerable<BMMenu> GetRoleManage(int seed)
        {
            yield return new BMMenu
            {
                //Id = GetGuid(seed + 5),
                Url = "/Role/Manage",
                Name = "角色管理",
                Key = ControllerConstants.RoleManage,
                IconUrl = IconType.Outline.UserSwitch,
            };

            yield return new BMMenu
            {
                //Id = GetGuid(seed + 10),
                Url = "/Role/Menu",
                Name = "角色菜单管理",
                Key = "RoleMenu",
                IconUrl = IconType.Outline.Menu,
            };
        }
    }

    protected virtual void SetUserIdAndTenantId(IEnumerable<BMMenu>? menus, Guid userId, Guid tenantId, long? sort = null)
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