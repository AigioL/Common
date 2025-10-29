using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class ImageController
{
    const int staticres_timeout_day = 30;

    public static void MapBasicImage<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "basic/image")
        where TAppSettings : class, IAppSettings
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("00000000-0000-0000-0000-000000000000", (HttpContext context) =>
        {
            var settings = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>().Value;
            var r = Get(settings);
            return r;
        }).WithDescription("默认 Guid.Empty 值返回固定默认图片")
            .Produces(StatusCodes.Status301MovedPermanently)
            .Produces(StatusCodes.Status404NotFound);
        routeGroup.MapGet("{id}", async (HttpContext context, [FromRoute] string id) =>
        {
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var repo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
            var r = await GetAsync<TAppSettings>(context, cache, repo, id, context.RequestAborted);
            return r;
        }).WithDescription("根据 ImageId 返回跳转的真实图片地址")
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound);
    }

    const string DefaultImageFileName = "00000000-0000-0000-0000-000000000000.png";

    /// <summary>
    /// 默认 <see cref="Guid.Empty"/> 值返回固定默认图片
    /// </summary>
    /// <typeparam name="TAppSettings"></typeparam>
    /// <param name="settings"></param>
    /// <returns></returns>
    static IResult Get<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        TAppSettings settings)
        where TAppSettings : class, IAppSettings
    {
        var settingImageUrl = settings.ImageUrl;
        if (!string.IsNullOrWhiteSpace(settingImageUrl))
        {
            var url = string.Format(settingImageUrl, DefaultImageFileName);
            return Results.Redirect(url, permanent: true);
        }
        return Results.NotFound();
    }

    /// <summary>
    /// 根据 ImageId 返回跳转的真实图片地址
    /// </summary>
    /// <typeparam name="TAppSettings"></typeparam>
    /// <param name="context"></param>
    /// <param name="cache"></param>
    /// <param name="repo"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<IResult> GetAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        HttpContext context,
        IDistributedCache cache,
        IStaticResourceRepository repo,
        string id,
        CancellationToken cancellationToken = default)
        where TAppSettings : class, IAppSettings
    {
        if (!ShortGuid.TryParse(id, out Guid idG))
        {
            return Results.NotFound();
        }

        var settings = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>().Value;

        if (idG == default)
        {
            return Get(settings);
        }

        string? url = null;

        var cacheKey = $"{nameof(ImageController)}_Get_{idG}";
        var entity = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(staticres_timeout_day);
            var r = await repo.FindAsync(idG, cancellationToken: cancellationToken);
            return r;
        }, cancellationToken);
        if (entity != null && !entity.SoftDeleted)
        {
            url = entity.Url;
            if (!url.IsHttpUrl())
            {
                var settingImageUrl = settings.ImageUrl;
                if (settingImageUrl != null)
                {
                    var fileName = entity.FileName;
                    url = string.Format(settingImageUrl, fileName);
                }
            }
        }

        if (url != null)
        {
            return Results.Redirect(url);
        }
        return Results.NotFound();
    }
}
