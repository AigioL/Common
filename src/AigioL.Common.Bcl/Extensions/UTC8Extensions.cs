#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

public static partial class UTC8Extensions
{
    /// <summary>
    /// 将时区转换为北京时间（UTC+8）
    /// </summary>
    public static DateTimeOffset ToUTC8(this DateTimeOffset dto)
    {
        var utc8Offset = TimeSpan.FromHours(8);
        if (dto.Offset == utc8Offset)
        {
            return dto;
        }
        return dto.ToOffset(utc8Offset);
    }

    /// <summary>
    /// 将时区转换为北京时间（UTC+8）并去除时间部分，保留日期部分
    /// </summary>
    public static DateTimeOffset ToUTC8Date(this DateTimeOffset dto)
    {
        dto = ToUTC8(dto);
        return new DateTimeOffset(dto.Year, dto.Month, dto.Day, 0, 0, 0, dto.Offset);
    }

    /// <summary>
    /// 将仅日期转换为北京时间（UTC+8）的 <see cref="DateTimeOffset"/>
    /// </summary>
    public static DateTimeOffset ToUTC8Date(this DateOnly @do)
    {
        var utc8Offset = TimeSpan.FromHours(8);
        return new DateTimeOffset(@do.Year, @do.Month, @do.Day, 0, 0, 0, utc8Offset);
    }

    /// <summary>
    /// 从 <see cref="DateTimeOffset"/> 中转换时区为北京时间（UTC+8）并提取日期部分，返回 <see cref="DateOnly"/>
    /// </summary>
    public static DateOnly GetDateOnly(this DateTimeOffset dto)
    {
        var date = ToUTC8Date(dto);
        return new DateOnly(date.Year, date.Month, date.Day);
    }

    public static string ToDateString(this DateTime dt) => dt.ToString("yyyy-MM-dd");

    public static string ToDateString(this DateTimeOffset dt) => dt.ToString("yyyy-MM-dd");
}
