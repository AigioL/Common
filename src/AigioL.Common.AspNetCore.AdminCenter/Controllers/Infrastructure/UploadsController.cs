using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Helpers.Uploads;
using AigioL.Common.Models;
using AigioL.Common.Storage.Services;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

public static partial class UploadsController
{
    const string ControllerName = ControllerConstants.Uploads;
    const string DefaultKeyPrefix = "baseDebug/File";

    public static void MapUploads(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/uploads")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("管理后台的上传文件相关接口");

        var upload = routeGroup.MapPost("", async (HttpContext context,
            [FromQuery] string? bucket = null,
            [FromQuery] string? keyPrefix = null,
            [FromQuery] bool useOriginal = false,
            [FromQuery] uint? resizeWidth = null,
            [FromQuery] uint? resizeHeight = null,
            [FromQuery] FilterType? resizeFilter = null,
            [FromQuery] MagickFormat setImageFormat = UploadHelper.DefaultSetImageFormat,
            [FromQuery] uint quality = UploadHelper.DefaultSetImageQuality) =>
        {
            var r = await UploadAsync(context, bucket, keyPrefix, useOriginal, resizeWidth, resizeHeight, resizeFilter, setImageFormat, quality);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("上传本地文件");

#if DEBUG
        var machineHash = Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(Environment.MachineName)));
        switch (machineHash)
        {
            case "ba13dd0ff3a5b9fe742eb4d21bfd159a":
                {
                    upload.AllowAnonymous();
                }
                break;
            default:
                break;
        }
#endif
    }

    public static async Task<BMApiRsp<UploadFileInfo?>> UploadAsync(
        HttpContext context,
        string? bucket = null,
        string? keyPrefix = null,
        bool useOriginal = false,
        uint? resizeWidth = null,
        uint? resizeHeight = null,
        FilterType? resizeFilter = null,
        MagickFormat setImageFormat = UploadHelper.DefaultSetImageFormat,
        uint quality = UploadHelper.DefaultSetImageQuality)
    {
        // 目前仅支持单个文件上传
        IFormFile file;
        try
        {
            var files = context.Request.Form.Files;
            if (files.Count != 1)
            {
                return HttpStatusCode.BadRequest;
            }
            file = context.Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                return HttpStatusCode.BadRequest;
            }
            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                return HttpStatusCode.BadRequest;
            }
        }
        catch (InvalidOperationException)
        {
            return HttpStatusCode.BadRequest;
        }

        if (string.IsNullOrWhiteSpace(keyPrefix))
        {
            keyPrefix = DefaultKeyPrefix;
        }

        var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
        var oss = context.RequestServices.GetRequiredService<IObjectStorageService>(); // 当前仅支持腾讯云对象存储

        using var stream = file.OpenReadStream();
        var fileEx = Path.GetExtension(file.FileName);
        var fileNameWithoutEx = Path.GetFileNameWithoutExtension(file.FileName);

        var (resultStream, _, info) = await UploadHelper.GetUploadFileInfoAsync(stream, fileNameWithoutEx, fileEx, useOriginal: useOriginal, resizeWidth: resizeWidth, resizeHeight: resizeHeight, resizeFilter: resizeFilter, setImageFormat: setImageFormat, quality: quality, leaveOpen: true, cancellationToken: context.RequestAborted);

        ArgumentException.ThrowIfNullOrWhiteSpace(info.SHA384);
        // 比较数据库中是否存在相同的文件，如果存在则直接返回数据库中的信息，而不再上传文件到对象存储
        var urlByFind = await staticResourceRepo.GetUrlByHashWithSizeAsync(info.SHA384, info.FileSize, context.RequestAborted);
        if (Uri.TryCreate(urlByFind, UriKind.RelativeOrAbsolute, out var urlByFind2))
        {
            info.Url = urlByFind2;
            return info;
        }

        var result = await oss.UploadAsync(bucket, keyPrefix, resultStream, fileNameWithoutEx, fileEx: fileEx, cancellationToken: context.RequestAborted);
        if (result.IsSuccess() && result.Content != null)
        {
            info.Url = result.Content;
            ReadOnlySpan<char> fileExSpan = info.FileEx;
            StaticResource entity = new()
            {
                FileName = info.FileName,
                SHA384 = info.SHA384,
                FileExtension = info.FileEx,
                FileSize = info.FileSize,
                FileType = fileExSpan.GetFileFormat() ?? default,
                Url = info.Url.ToString(),
                CreateUserId = context.GetBMUserId(),
            };

            // 上传成功，写入数据库
            await staticResourceRepo.InsertAsync(entity, CancellationToken.None);
            return info;
        }
        else
        {
            return result.CreateNew<UploadFileInfo?>(info);
        }
    }
}
