namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 手机号码
/// </summary>
public interface IPhoneNumber : IReadOnlyPhoneNumber
{
    /// <summary>
    /// 国家或地区代码
    /// </summary>
    new string? PhoneNumberRegionCode { get; set; }

    /// <inheritdoc cref="IPhoneNumber"/>
    new string? PhoneNumber { get; set; }

    const int DatabaseMaxLength = 16;
    const int RegionCodeDatabaseMaxLength = 8;

    /// <summary>
    /// 默认国家或地区代码
    /// </summary>
    const string DefaultPhoneNumberRegionCode = "+86";

    /// <summary>
    /// 代表一个隐藏字符，以保护用户隐私或防止信息泄露
    /// </summary>
    const char HideChar = '*';

    /// <summary>
    /// Android 模拟器默认电话号码，硬编码屏蔽此号
    /// </summary>
    const string SimulatorDefaultValue = "15555218135";

    /// <summary>
    /// 手机号码隐藏中间四位数字
    /// <para>实现逻辑：字符串大于等于 7 时，将下标 3~6 替换为隐藏字符</para>
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="hideChar"></param>
    /// <returns></returns>
    static string ToStringHideMiddleFour(string? phoneNumber, char hideChar = HideChar)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            if (phoneNumber.Length >= 7)
            {
                Span<char> array = stackalloc char[phoneNumber.Length];
                phoneNumber.AsSpan().CopyTo(array);
                array[3] = hideChar;
                array[4] = hideChar;
                array[5] = hideChar;
                array[6] = hideChar;
                return new string(array);
            }
            return phoneNumber;
        }
        return string.Empty;
    }
}

/// <inheritdoc cref="IPhoneNumber"/>
public interface IReadOnlyPhoneNumber
{
    /// <inheritdoc cref="IPhoneNumber.PhoneNumberRegionCode"/>
    string? PhoneNumberRegionCode { get; }

    /// <inheritdoc cref="IPhoneNumber"/>
    string? PhoneNumber { get; }
}