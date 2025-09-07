using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Shared;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Text.Encodings.Web;

#pragma warning disable IDE1006 // 命名样式
namespace AigioL.Common.AspNetCore.AppCenter.Identity.Views.Shared;

public abstract class _Layout<TModel> : IResult
{
    protected readonly TModel model;
    protected readonly JavaScriptEncoder encoder;
    protected readonly int statusCode;
    protected readonly bool useResponseCompression;

    protected const int DefaultStatusCode = StatusCodes.Status200OK;
    protected const bool DefaultUseResponseCompression = true;

    public _Layout(TModel model, JavaScriptEncoder? encoder = null, int statusCode = DefaultStatusCode, bool useResponseCompression = DefaultUseResponseCompression)
    {
        this.model = model;
        this.encoder ??= JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        this.statusCode = statusCode;
        this.useResponseCompression = useResponseCompression;
    }

    protected abstract LayoutModel GetLayoutModel();

    protected abstract Task RenderSectionAsync(HttpContext context, string sectionName);

    protected abstract Task RenderBodyAsync(HttpContext context);

    protected const string Styles = "Styles";
    protected const string Scripts = "Scripts";

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "text/html; charset=utf-8";

        if (useResponseCompression)
        {
            // https://learn.microsoft.com/zh-cn/aspnet/core/performance/response-compression?view=aspnetcore-9.0#providers
            // https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Middleware/ResponseCompression/src/ResponseCompressionMiddleware.cs
            var provider = ResponseCompression.GetProvider(context.RequestServices);
            await new ResponseCompressionMiddleware(ExecuteCoreAsync, provider).Invoke(context);
        }
        else
        {
            await ExecuteCoreAsync(context);
        }
    }

    async Task ExecuteCoreAsync(HttpContext context)
    {
        var model = GetLayoutModel();
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
<!DOCTYPE html><html lang="
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.HtmlLang, encoder);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
"><head><meta charset="utf-8" />
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<meta name="referrer" content="never">
<meta http-equiv="Cache-Control" content="no-siteapp" />
<meta name="renderer" content="webkit">
<meta http-equiv="X-UA-Compatible" content="IE=Edge,chrome=1">
<meta name="wap-font-scale" content="no">
<meta name="theme-color" content="
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.MetaThemeColor, encoder);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
">
<meta name="msapplication-TileColor" content="
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.MetaMSApplicationTileColor, encoder);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
">
<meta name="msapplication-window" content="
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.MetaMSApplicationWindow, encoder);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
">
<meta name="mobile-web-app-capable" content="yes">
<meta name="keywords" content="
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.MetaKeywords, encoder);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
" />
<meta name="description" content="
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.MetaDescription, encoder);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
" />
<title>
"""u8);
        context.Response.BodyWriter.WriteHtmlEncodedText(model.HeadTitle);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
</title>
"""u8);
        await RenderSectionAsync(context, Styles);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
</head>
<body>
<noscript>
"""u8);
        context.Response.BodyWriter.WriteHtmlEncodedText(model.NoScript);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
</noscript>
"""u8);
        await RenderBodyAsync(context);
        await RenderSectionAsync(context, Scripts);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
</body>
</html>
"""u8);
        await context.Response.BodyWriter.FlushAsync(context.RequestAborted);

    }
}

file static class ResponseCompression
{
    static IResponseCompressionProvider? provider;

    internal static IResponseCompressionProvider GetProvider(IServiceProvider serviceProvider)
    {
        if (provider == null)
        {
            // https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Middleware/ResponseCompression/src/ResponseCompressionServicesExtensions.cs#L40
            provider = serviceProvider.GetService<IResponseCompressionProvider>();
            // 使用默认配置创建提供程序
            provider ??= new ResponseCompressionProvider(serviceProvider, Options.Create(new ResponseCompressionOptions()));
        }
        return provider;
    }
}