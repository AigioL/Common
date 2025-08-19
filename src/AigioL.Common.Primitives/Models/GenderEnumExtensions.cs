using System.Runtime.CompilerServices;

namespace AigioL.Common.Primitives.Models;

/// <summary>
/// Enum 扩展 <see cref="Gender"/>
/// </summary>
public static partial class GenderEnumExtensions
{
    /// <summary>
    /// 性别为男（Male）或女（Female）
    /// </summary>
    /// <param name="gender"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMaleOrFemale(this Gender gender)
        => gender == Gender.Male || gender == Gender.Female;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Gender ParseGender(string? str)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            switch (str[0])
            {
                case '男':
                    return Gender.Male;
                case '女':
                    return Gender.Female;
                default:
                    {
                        if (Enum.TryParse<Gender>(str, true, out var result))
                        {
                            return result;
                        }
                    }
                    break;
            }
        }
        return Gender.Unknown;
    }
}
