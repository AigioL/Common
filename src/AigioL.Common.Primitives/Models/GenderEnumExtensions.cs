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
}
