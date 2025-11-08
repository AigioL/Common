using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.Helpers.ProgramMain.Controllers.Infrastructure;

public static partial class InfoController
{
    public static void MapGetInfo(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "{projId}/info")
    {
        pattern = ProgramHelper.GetEndpointPattern(pattern);

        b.MapGet(pattern, async (HttpContext context) =>
        {
            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

            if (!env.IsDevelopment())
            {
                return Results.Content($"{ProgramHelper.ProjectName} v{ProgramHelper.Version}");
            }

            var ip = context.Connection.RemoteIpAddress?.ToString();
            var now = DateTimeOffset.Now;
            var timeZoneInfo = TimeZoneInfo.Local;

            var partManager = context.RequestServices.GetService<ApplicationPartManager>();
            InfoModel.ApplicationPartsModel? applicationParts;
            if (partManager != null)
            {
                var controllerFeature = new ControllerFeature();
                partManager.PopulateFeature(controllerFeature);
                var tagHelperFeature = new TagHelperFeature();
                partManager.PopulateFeature(tagHelperFeature);
                var viewComponentFeature = new ViewComponentFeature();
                partManager.PopulateFeature(viewComponentFeature);
                applicationParts = new()
                {
                    Controllers = [.. controllerFeature.Controllers.Select(c => c.Name)],
                    TagHelpers = [.. tagHelperFeature.TagHelpers.Select(t => t.Name)],
                    ViewComponents = [.. viewComponentFeature.ViewComponents.Select(v => v.Name)],
                };
            }
            else
            {
                applicationParts = null;
            }


            string?[]? dbVersion = null;
            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(3.1));
            cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, context.RequestAborted);
            try
            {
                var db = context.RequestServices.GetService<ProgramHelper.IDbContext>()?.GetDbContext();
                if (db != null)
                {
                    dbVersion = [(await db.Database.SqlQueryRaw<VersionEntity>("SELECT version()").FirstOrDefaultAsync(cts.Token))?.version];
                }
            }
            catch (Exception ex)
            {
                dbVersion = ex.ToString()?.Split(Environment.NewLine);
            }

            string hostName = Dns.GetHostName();
            var hostAddresses = await Dns.GetHostAddressesAsync(hostName);
            EnvironmentInfo e = new();
            InfoModel m = new()
            {
                IpAddress = ip,
                ProjectName = ProgramHelper.ProjectName,
                Version = ProgramHelper.Version,
                RuntimeVersion = Environment.Version.ToString(),
                //CentralProcessorName = $"{ProgramHelper.CentralProcessorName} x{Environment.ProcessorCount}",
                WebRootPath = env.WebRootPath,
                ContentRootPath = env.ContentRootPath,
                EnvironmentName = env.EnvironmentName,
                ApplicationName = env.ApplicationName,
                CurrentCulture = CultureInfo.CurrentCulture,
                CurrentUICulture = CultureInfo.CurrentUICulture,
                //RawUrl = context.Request.RawUrl(),
                Protocol = context.Request.Protocol,
                //UserHostAddress = Request.UserHostAddress(),
                UserAgent = context.Request.Headers.UserAgent,
                AcceptLanguage = context.Request.Headers.AcceptLanguage,
                Now = now,
                DayOfWeek = now.DayOfWeek,
                TimeZoneInfo = timeZoneInfo,
                ApplicationParts = applicationParts,
                DatabaseVersion = dbVersion,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                OSDescription = RuntimeInformation.OSDescription,
                OSArchitecture = RuntimeInformation.OSArchitecture,
                Containerized = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") is not null,
                UserName = Environment.UserName,
                MemoryLimit = IOPath.GetDisplayFileSizeString(e.MemoryLimit),
                MemoryUsage = IOPath.GetDisplayFileSizeString(e.MemoryUsage),
                TotalAvailableMemoryBytes = IOPath.GetDisplayFileSizeString(e.TotalAvailableMemoryBytes),
                HostName = hostName,
                HostAddresses = [.. hostAddresses.Select(x => x.ToString())],
                IsDynamicCodeCompiled = RuntimeFeature.IsDynamicCodeCompiled,
                IsDynamicCodeSupported = RuntimeFeature.IsDynamicCodeSupported,
            };
            return Results.Json(m, InfoController_InfoModel_JSC.Default.InfoModel);
        })
        .AllowAnonymous()
        .WithDescription("测试输出信息");
    }

    sealed record class VersionEntity
    {
#pragma warning disable IDE1006 // 命名样式
        public string? version { get; init; }
#pragma warning restore IDE1006 // 命名样式
    }

    internal sealed record class InfoModel
    {
        public string? IpAddress { get; set; }

        public string? ProjectName { get; set; }

        public string? Version { get; set; }

        public string? RuntimeVersion { get; set; }

        //public string? CentralProcessorName { get; set; }

        public string? WebRootPath { get; set; }

        public string? ContentRootPath { get; set; }

        public string? EnvironmentName { get; set; }

        public string? ApplicationName { get; set; }

        public CultureInfoModel? CurrentCulture { get; set; }

        public CultureInfoModel? CurrentUICulture { get; set; }

        //public string? RawUrl { get; set; }

        public string? Protocol { get; set; }

        //public string? UserHostAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? AcceptLanguage { get; set; }

        public DateTimeModel? Now { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeZoneInfoModel? TimeZoneInfo { get; set; }

        public ApplicationPartsModel? ApplicationParts { get; set; }

        public string?[]? DatabaseVersion { get; set; }

        public string? FrameworkDescription { get; set; }

        public string? OSDescription { get; set; }

        public Architecture OSArchitecture { get; set; }

        public bool Containerized { get; set; }

        public string? UserName { get; set; }

        public string? MemoryLimit { get; set; }

        public string? MemoryUsage { get; set; }

        public string? TotalAvailableMemoryBytes { get; set; }

        public string? HostName { get; set; }

        public string[]? HostAddresses { get; set; }

        public bool IsDynamicCodeCompiled { get; set; }

        public bool IsDynamicCodeSupported { get; set; }

        public sealed record class CultureInfoModel
        {
            public int LCID { get; set; }

            public string? Name { get; set; }

            public string? NativeName { get; set; }

            public string? DisplayName { get; set; }

            public string? EnglishName { get; set; }

            public string? TwoLetterISOLanguageName { get; set; }

            public string? ThreeLetterISOLanguageName { get; set; }

            public string? ThreeLetterWindowsLanguageName { get; set; }

            public static implicit operator CultureInfoModel(CultureInfo cultureInfo) => new()
            {
                LCID = cultureInfo.LCID,
                Name = cultureInfo.Name,
                NativeName = cultureInfo.NativeName,
                DisplayName = cultureInfo.DisplayName,
                EnglishName = cultureInfo.EnglishName,
                TwoLetterISOLanguageName = cultureInfo.TwoLetterISOLanguageName,
                ThreeLetterISOLanguageName = cultureInfo.ThreeLetterISOLanguageName,
                ThreeLetterWindowsLanguageName = cultureInfo.ThreeLetterWindowsLanguageName,
            };
        }

        public sealed record class DateTimeModel
        {
            public DateTimeOffset Default { get; set; }

            public string? RFC1123 { get; set; }

            public string? Standard { get; set; }

            public static implicit operator DateTimeModel(DateTimeOffset d) => new()
            {
                Default = d,
                RFC1123 = d.ToString("r"),
                Standard = d.ToString("yyyy-MM-dd HH:mm:ss"),
            };
        }

        public sealed record class TimeZoneInfoModel
        {
            public string? Id { get; set; }

            public string? DisplayName { get; set; }

            public string? StandardName { get; set; }

            public string? DaylightName { get; set; }

            public TimeSpan BaseUtcOffset { get; set; }

            public static implicit operator TimeZoneInfoModel(TimeZoneInfo t) => new()
            {
                Id = t.Id,
                DisplayName = t.DisplayName,
                StandardName = t.StandardName,
                DaylightName = t.DaylightName,
                BaseUtcOffset = t.BaseUtcOffset,
            };
        }

        public sealed record class ApplicationPartsModel
        {
            public string[]? Controllers { get; set; }

            public string[]? TagHelpers { get; set; }

            public string[]? ViewComponents { get; set; }
        }
    }

    /// <summary>
    /// https://github.com/dotnet/dotnet-docker/blob/334580f27f92b87a54fd7f46ee46a6557a26bf86/samples/aspnetapp/aspnetapp/EnvironmentInfo.cs
    /// </summary>
    readonly struct EnvironmentInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentInfo"/> struct.
        /// </summary>
        public EnvironmentInfo()
        {
            GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
            TotalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            string[] memoryLimitPaths =
            [
                "/sys/fs/cgroup/memory.max",
                "/sys/fs/cgroup/memory.high",
                "/sys/fs/cgroup/memory.low",
                "/sys/fs/cgroup/memory/memory.limit_in_bytes",
            ];

            string[] currentMemoryPaths =
            [
                "/sys/fs/cgroup/memory.current",
                "/sys/fs/cgroup/memory/memory.usage_in_bytes",
            ];

            MemoryLimit = GetBestValue(memoryLimitPaths);
            MemoryUsage = GetBestValue(currentMemoryPaths);
        }

        public long TotalAvailableMemoryBytes { get; }

        public long MemoryLimit { get; }

        public long MemoryUsage { get; }

        static long GetBestValue(string[] paths)
        {
            foreach (string path in paths)
            {
                if (Path.Exists(path) &&
                    long.TryParse(global::System.IO.File.ReadAllText(path), out long result))
                {
                    return result;
                }
            }

            return 0;
        }
    }
}

[JsonSerializable(typeof(InfoController.InfoModel))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true)]
sealed partial class InfoController_InfoModel_JSC : JsonSerializerContext
{
    static InfoController_InfoModel_JSC()
    {
        // https://github.com/dotnet/runtime/issues/94135
        JsonSerializerOptions o = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,

            #region JsonSerializerDefaults.Web https://github.com/dotnet/runtime/blob/v9.0.7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L172-L174

            PropertyNameCaseInsensitive = true, // 忽略大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // 允许从字符串读取数字

            #endregion

        };
        Default = new InfoController_InfoModel_JSC(o);
    }
}