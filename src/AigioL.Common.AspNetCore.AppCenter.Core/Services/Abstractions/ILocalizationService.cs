using System.Globalization;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

public partial interface ILocalizationService
{
    /// <summary>
    /// 获取语言键，值格式为 <see cref="CultureInfo.Name"/>
    /// </summary>
    /// <returns></returns>
    string? GetLangKey();

    /// <summary>
    /// 获取默认的语言键，值格式为 <see cref="CultureInfo.Name"/>
    /// </summary>
    /// <returns></returns>
    string GetDefaultLangKey();
}
