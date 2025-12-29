using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage.AddOrEditStaticResourceModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage.StaticResourceTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 静态资源管理
/// </summary>
public static partial class StaticResourceController
{
    const string ControllerName = ControllerConstants.StaticResource;

    public static void MapStaticResource(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/staticresource",
        bool addLocalTest = false,
        int bufferBodyLengthLimit = MaxLengths.OneGBInBytes,
        int multipartBodyLengthLimit = MaxLengths.OneGBInBytes)
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("静态资源管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? fileName,
            [FromQuery] string? filePath,
            [FromQuery] CloudFileType? fileType,
            [FromQuery] string? sha384,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await staticResourceRepo.QueryAsync(
                fileName, filePath, fileType,
                sha384, orderBy, desc,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询静态资源");

        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] AddOrEditM model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var userId = context.GetBMUserId();
            var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
            BMApiRsp r = await staticResourceRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改静态资源");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
            BMApiRsp r = await staticResourceRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增静态资源");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
            var rowCount = await staticResourceRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除静态资源");

        routeGroup.MapPost("upload", async (HttpContext context) =>
        {
            var userId = context.GetBMUserId();
            var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();
            if (context.Request.Form.Files.Count == 0)
            {
                return "文件上传失败，缺少需要上传的有效文件";
            }

            var results = new StaticResourceUploadResult[context.Request.Form.Files.Count];
            for (int i = 0; i < context.Request.Form.Files.Count; i++)
            {
                var file = context.Request.Form.Files[i];
                var extension = Path.GetExtension(file.FileName);
                using var s = file.OpenReadStream();

                // 检查文件是否为图片类型
                //if (FileFormat.IsImage(s, out var imageFormat))
                //{
                //    if (!imageFormat.IsAllow())
                //    {
                //        responseItems.Add(new() { FileName = file.FileName, Code = UploadFileItemCodes.FilesUnsupportedType });
                //        break;
                //    }
                //    extension = imageFormat.GetExtension();
                //}

                //var fileType = extension.GetFileFormat(imageFormat.IsDefined() ? imageFormat : null);
                //// 不允许的文件类型
                //if (!fileType.HasValue || !fileType.Value.IsDefined())
                //{
                //    responseItems.Add(new() { FileName = file.FileName, Code = UploadFileItemCodes.FilesUnsupportedType });
                //    break;
                //}

                //s.Position = 0;
                //var sha384 = await Hashs.String.SHA384Async(s, cancellationToken: HttpContext.RequestAborted);
                //s.Position = 0;
                //var sha256 = await Hashs.String.SHA256Async(s, cancellationToken: HttpContext.RequestAborted);
                //var resource = await staticResourceRepo.FindAsync(sha384);

                //if (resource != null)
                //{
                //    await staticResourceUploadRecordRepo.InsertAsync(new StaticResourceUploadRecord
                //    {
                //        StaticResourceId = resource.Id,
                //        TenantId = TenantConstants.RootTenantIdG,
                //        CreateUserId = userId,
                //        CreationTime = DateTimeOffset.Now,
                //    });
                //    responseItems.Add(new StaticResourceUploadResult
                //    {
                //        FileName = file.FileName,
                //        Url = resource.Url,
                //        Code = UploadFileItemCodes.Ok,
                //        StaticResourceId = resource.Id,
                //        SHA384 = resource.SHA384,
                //    });
                //    break;
                //}
                //else
                //    resource = new StaticResource
                //    {
                //        FileExtension = extension,
                //        FileName = file.FileName,
                //        FileSize = file.Length,
                //        FileType = fileType.Value,
                //        SHA256 = sha256,
                //        SHA384 = sha384,
                //        TenantId = TenantConstants.RootTenantIdG,
                //        CreateUserId = userId,
                //        CreationTime = DateTimeOffset.Now,
                //    };
                //var (url, path) = await fileService.SaveFile(s, fileType.Value.ToString(), sha384 + extension);
                //if (url == null)
                //{
                //    responseItems.Add(new() { FileName = file.FileName, Code = UploadFileItemCodes.SaveFileFailure });
                //    break;
                //}
                //else
                //{
                //    resource.Url = url;
                //    resource.FilePath = path;
                //    var state = await staticResourceRepo.InsertAsync(resource) > 0;
                //    if (state)
                //        await staticResourceUploadRecordRepo.InsertAsync(new StaticResourceUploadRecord
                //        {
                //            CreateUserId = userId,
                //            StaticResourceId = resource.Id,
                //            CreationTime = DateTimeOffset.Now,
                //            TenantId = TenantConstants.RootTenantIdG
                //        });
                //    responseItems.Add(new()
                //    {
                //        FileName = file.FileName,
                //        Url = resource?.Url,
                //        Code = state ? UploadFileItemCodes.Ok : UploadFileItemCodes.InsertDataBaseFailure,
                //        StaticResourceId = resource!.Id,
                //        SHA384 = resource.SHA384,
                //    });
                //}
            }

            BMApiRsp<StaticResourceUploadResult[]> r = results;
            return r;
        })
        .WithFormOptions(
            bufferBodyLengthLimit: bufferBodyLengthLimit, // [RequestSizeLimit(1024 * 1024 * 1024)]
            multipartBodyLengthLimit: multipartBodyLengthLimit // [RequestFormLimits(MultipartBodyLengthLimit = 1024 * 1024 * 1024)]
        )
        .WithDescription("上传静态资源文件");

        if (addLocalTest)
        {
            routeGroup.MapGet("local/{fileName}", async (HttpContext context,
                [FromRoute] string fileName) =>
            {
                if (fileName.Length > MaxLengths.SHA384)
                {
                    var sha384 = Path.GetFileNameWithoutExtension(fileName);
                    if (sha384.Length == MaxLengths.SHA384)
                    {
                        var fileExt = fileName[MaxLengths.SHA384..];
                        var userId = context.GetBMUserId();
                        var staticResourceRepo = context.RequestServices.GetRequiredService<IStaticResourceRepository>();

                        var (filePath, fileType) = await staticResourceRepo.GetFilePathBySha384WithFileExtAsync(sha384, fileExt, context.RequestAborted);
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            return Results.File(filePath);
                        }
                    }
                }
                return Results.NotFound();
            })
            .AllowAnonymous()
            .WithDescription("本地静态资源访问");
        }
    }
}
