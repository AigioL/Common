using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class ImageController
{
    public static void MapBasicImage<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/image")
        where TAppSettings : class, IAppSettings
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("00000000-0000-0000-0000-000000000000", (HttpContext context) =>
        {
            var r = Get<TAppSettings>(context);
            return r;
        }).WithDescription("默认 Guid.Empty 值返回固定默认图片")
            .Produces(StatusCodes.Status301MovedPermanently)
            .Produces(StatusCodes.Status404NotFound);
        routeGroup.MapGet("{id}", async (HttpContext context, [FromRoute] string id) =>
        {
            var r = await GetAsync<TAppSettings>(context, id);
            return r;
        }).WithDescription("根据 ImageId 返回跳转的真实图片地址")
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound);
    }

    const string DefaultImageFileName = "00000000-0000-0000-0000-000000000000.png";

    /// <summary>
    /// 默认 <see cref="Guid.Empty"/> 值返回固定默认图片
    /// </summary>
    /// <returns></returns>
    static IResult Get<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        HttpContext context)
        where TAppSettings : class, IAppSettings
    {
        var settings = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>().Value;
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
    /// <param name="id"></param>
    /// <returns></returns>
    static async Task<IResult> GetAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        HttpContext context, string id)
        where TAppSettings : class, IAppSettings
    {
        if (!ShortGuid.TryParse(id, out Guid guid))
        {
            return Results.NotFound();
        }
        if (guid == default)
        {
            return Get<TAppSettings>(context);
        }

        // TODO: 实现图片访问

        string? url;
        throw new NotImplementedException("TODO: 实现图片访问");
        await Task.CompletedTask;
        //var entity = await memoryCache.GetOrCreateAsync($"{nameof(ImageController)}_Get_{id}", async entry =>
        //{
        //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
        //    var r = await staticResourceRepo.FindAsync(id, cancellationToken: HttpContext.RequestAborted);
        //    return r;
        //});
        //if (entity != null && !entity.SoftDeleted)
        //{
        //    if (!Enum.IsDefined(entity.FileType.GetImageFormat())) // 只允许图片
        //    {
        //        return NotFound();
        //    }

        //    url = entity.Url;
        //    if (!String2.IsHttpUrl(url, true))
        //    {
        //        var fileName = entity.FileName;
        //        var settingImageUrl = settings.ImageUrl;
        //        url = string.Format(settingImageUrl.ThrowIsNull(), fileName);
        //    }
        //    return Redirect(url);
        //}
        //return NotFound();
    }
}
