using AigioL.Common.SmsSender.Models.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization.Metadata;

namespace AigioL.Common.SmsSender.Services.Implementation.SmsSender;

/// <summary>
/// Sms 发送基类
/// </summary>
public abstract class SmsSenderBase : ISmsSender
{
    /// <inheritdoc/>
    public abstract string Channel { get; }

    /// <inheritdoc/>
    public abstract bool SupportCheck { get; }

    /// <inheritdoc/>
    public abstract Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken);

    /// <summary>
    /// 生成随机短信验证码值，某些平台可能提供了随机生成，可以重写该函数替换
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public virtual string GenerateRandomNum(int length)
    {
        return GenerateRandomNum(length).ToString();
    }

    /// <summary>
    /// 生成随机数字，长度为固定传入参数
    /// </summary>
    /// <param name="length">要生成的字符串长度</param>
    /// <param name="endIsZero">生成的数字最后一位是否能够为0，默认不能为0( <see langword="false"/> )</param>
    /// <returns></returns>
    internal static int GenerateRandomNum(int length = 6, bool endIsZero = false)
    {
        if (length > 11) length = 11;
        var result = 0;
        var lastNum = 0;
        if (RandomNumberGenerator.GetInt32(256) % 2 == 0)
            for (int i = length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(i);
        else
            for (int i = 0; i < length; i++) // 0 1 2 3 4 5
                EachGenerate(i);
        return result;
        void EachGenerate(int i)
        {
            var bit = (int)(i == 0 ? 1 : Math.Pow(10, i));
            // 100,000  10,000  1,000   100     10      1
            // 1        10      100     1,000   10,000  100,000
            var current = RandomNumberGenerator.GetInt32(lastNum + 1, lastNum + 10);
            lastNum = current % 10;
            if (lastNum == 0)
            {
                // i != 0 &&  i!=5 末尾和开头不能有零
                if ((i != 0 || endIsZero) && i != length - 1)
                    return;
                lastNum = RandomNumberGenerator.GetInt32(1, 10);
            }
            result += lastNum * bit;
        }
    }

    /// <summary>
    /// 生成随机字符串，长度为固定传入字符串
    /// </summary>
    /// <param name="length">要生成的字符串长度</param>
    /// <param name="randomChars">随机字符串字符集</param>
    /// <returns></returns>
    internal static string GenerateRandomString(int length = 6,
        string randomChars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
    {
        var result = new char[length];
        if (RandomNumberGenerator.GetInt32(256) % 2 == 0)
            for (var i = length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(i);
        else
            for (var i = 0; i < length; i++) // 0 1 2 3 4 5
                EachGenerate(i);
        return new string(result);
        void EachGenerate(int i)
        {
            var index = RandomNumberGenerator.GetInt32(0, randomChars.Length);
            var temp = RandomCharAt(randomChars, index);
            static char RandomCharAt(string s, int index)
            {
                if (index == s.Length) index = 0;
                else if (index > s.Length) index %= s.Length;
                return s[index];
            }
            result[i] = temp;
        }
    }

    /// <summary>
    /// 依赖注入，添加短信发送服务
    /// </summary>
    /// <typeparam name="TSmsSender"></typeparam>
    /// <typeparam name="TSmsSettings"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSmsSender, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TSmsSettings>(
        IServiceCollection services)
        where TSmsSender : class, ISmsSender
        where TSmsSettings : class, ISmsSettings
    {
        services.AddHttpClient<TSmsSender>();
        services.AddScoped<DebugSmsSenderProvider>();
        services.AddScoped<ISmsSender, SmsSenderWrapper<TSmsSender, TSmsSettings>>();
        return services;
    }

    /// <summary>
    /// 依赖注入，根据短信渠道添加短信发送服务
    /// </summary>
    /// <typeparam name="TSmsSettings"></typeparam>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static IServiceCollection Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TSmsSettings>(
        IServiceCollection services,
        string? name)
        where TSmsSettings : class, ISmsSettings => name switch
        {
            null => services.AddScoped<ISmsSender, DebugSmsSenderProvider>(),
            nameof(ISmsSettings.SmsOptions._21VianetBlueCloud)
                => Add<Channels._21VianetBlueCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.AlibabaCloud)
                => Add<Channels.AlibabaCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.NetEaseCloud)
                => Add<Channels.NetEaseCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.HuaweiCloud)
                => Add<Channels.HuaweiCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.TencentCloud)
                => Add<Channels.TencentCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };

    /// <summary>
    /// 获取 JSON 格式的 HttpContent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inputValue"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <returns></returns>
    protected virtual HttpContent GetJsonContent<T>(T inputValue, JsonTypeInfo<T> jsonTypeInfo)
    {
        var content = JsonContent.Create(inputValue, jsonTypeInfo);
        return content;
    }

    /// <summary>
    /// 从 HttpContent 中读取 JSON 数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<T?> ReadFromJsonAsync<T>(HttpContent content, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken)
    {
        var jsonObj = await content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
        return jsonObj;
    }
}