using AigioL.Common.AspNetCore.AppCenter.Security;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class SecurityKeyBuilderExtensions
{
    /// <summary>
    /// 添加 SecurityKey 模式的中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSecurityKey(this IApplicationBuilder app)
        => AddSecurityKeyMiddleware(app, Options.Create(new DefaultSecurityKeyOptions()));

    /// <summary>
    /// 使用泛型配置项添加 SecurityKey 模式的中间件
    /// </summary>
    /// <typeparam name="TSecurityKeyOptions"></typeparam>
    /// <param name="app"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSecurityKey<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSecurityKeyOptions>(
        this IApplicationBuilder app,
        TSecurityKeyOptions? options = null)
        where TSecurityKeyOptions : class, ISecurityKeyOptions
        => AddSecurityKeyMiddleware(app, options == null ? null : Options.Create(options));

    static IApplicationBuilder AddSecurityKeyMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSecurityKeyOptions>(
        IApplicationBuilder app,
        IOptions<TSecurityKeyOptions>? options)
        where TSecurityKeyOptions : class, ISecurityKeyOptions => app.Use(next =>
        {
            options ??= app.ApplicationServices.GetRequiredService<IOptions<TSecurityKeyOptions>>();
            return new SecurityKeyMiddleware<TSecurityKeyOptions>(next, options).Invoke;
        });
}

file sealed class DefaultSecurityKeyOptions : ISecurityKeyOptions
{
    public byte[]? ECDH_SharedKey { get; set; }
}