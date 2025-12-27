using System.ComponentModel;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

public static partial class EnumExtensions
{
    /// <summary>
    /// 返回指定枚举值的描述（通过
    /// <see cref="DescriptionAttribute"/> 指定）
    /// 如果没有指定描述，则返回枚举常数的名称，没有找到枚举常数则返回枚举值
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="value">要获取描述的枚举值</param>
    /// <returns>指定枚举值的描述</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetDescription<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        // 获取枚举常数名称
#if NET5_0_OR_GREATER
        var name = Enum.GetName(value);
#else
        var name = Enum.GetName(value.GetType(), value);
#endif
        if (name != null)
        {
            // 获取枚举字段
            var enumType = value.GetType();
            var fieldInfo = enumType.GetField(name);
            if (fieldInfo != null)
            {
                if (Attribute.GetCustomAttribute(fieldInfo,
                    typeof(DescriptionAttribute), false) is DescriptionAttribute description)
                {
                    return description.Description;
                }
            }
        }
        return null;
    }

    /// <inheritdoc cref="GetDescription{TEnum}(TEnum)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetDescription(this Enum value)
    {
        var enumType = value.GetType();
        // 获取枚举常数名称
        var name = Enum.GetName(enumType, value);
        if (name != null)
        {
            // 获取枚举字段
            var fieldInfo = enumType.GetField(name);
            if (fieldInfo != null)
            {
                if (Attribute.GetCustomAttribute(fieldInfo,
                    typeof(DescriptionAttribute), false) is DescriptionAttribute description)
                {
                    return description.Description;
                }
            }
        }
        return null;
    }
}
