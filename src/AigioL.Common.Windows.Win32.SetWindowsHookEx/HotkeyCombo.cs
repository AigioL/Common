#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键组合键，表示修饰键和主键的组合
/// </summary>
public readonly record struct HotkeyCombo(ModifierKeys Modifiers, VirtualKey Key)
{
    /// <summary>
    /// 获取热键组合的字符串表示
    /// </summary>
    /// <returns>格式化的热键组合字符串</returns>
    public override string ToString()
    {
        var parts = new List<string>();

        // 添加修饰键
        if (Modifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");

        if (Modifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");

        if (Modifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");

        if (Modifiers.HasFlag(ModifierKeys.Win))
            parts.Add("Win");

        // 添加主键
        parts.Add(GetKeyDisplayName(Key));

        return string.Join(" + ", parts);
    }

    /// <summary>
    /// 获取按键的显示名称
    /// </summary>
    /// <param name="key">虚拟键码</param>
    /// <returns>按键的友好显示名称</returns>
    static string GetKeyDisplayName(VirtualKey key)
    {
        return key switch
        {
            // 功能键
            VirtualKey.F1 => "F1",
            VirtualKey.F2 => "F2",
            VirtualKey.F3 => "F3",
            VirtualKey.F4 => "F4",
            VirtualKey.F5 => "F5",
            VirtualKey.F6 => "F6",
            VirtualKey.F7 => "F7",
            VirtualKey.F8 => "F8",
            VirtualKey.F9 => "F9",
            VirtualKey.F10 => "F10",
            VirtualKey.F11 => "F11",
            VirtualKey.F12 => "F12",

            // 字母键
            >= VirtualKey.A and <= VirtualKey.Z => key.ToString(),

            // 数字键
            VirtualKey.D0 => "0",
            VirtualKey.D1 => "1",
            VirtualKey.D2 => "2",
            VirtualKey.D3 => "3",
            VirtualKey.D4 => "4",
            VirtualKey.D5 => "5",
            VirtualKey.D6 => "6",
            VirtualKey.D7 => "7",
            VirtualKey.D8 => "8",
            VirtualKey.D9 => "9",

            // 小键盘
            VirtualKey.NumPad0 => "NumPad 0",
            VirtualKey.NumPad1 => "NumPad 1",
            VirtualKey.NumPad2 => "NumPad 2",
            VirtualKey.NumPad3 => "NumPad 3",
            VirtualKey.NumPad4 => "NumPad 4",
            VirtualKey.NumPad5 => "NumPad 5",
            VirtualKey.NumPad6 => "NumPad 6",
            VirtualKey.NumPad7 => "NumPad 7",
            VirtualKey.NumPad8 => "NumPad 8",
            VirtualKey.NumPad9 => "NumPad 9",

            // 特殊键
            VirtualKey.Space => "Space",
            VirtualKey.Enter => "Enter",
            VirtualKey.Escape => "Esc",
            VirtualKey.Tab => "Tab",

            // 默认情况
            _ => key.ToString()
        };
    }

    /// <summary>
    /// 检查当前热键组合是否与指定的修饰键和按键匹配
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <returns>如果匹配返回 true，否则返回 false</returns>
    public bool Matches(ModifierKeys modifiers, VirtualKey key)
    {
        return Modifiers == modifiers && Key == key;
    }

    /// <summary>
    /// 检查热键组合是否有效
    /// </summary>
    /// <returns>如果热键组合有效返回 true，否则返回 false</returns>
    public bool IsValid()
    {
        // 至少需要一个修饰键（除了某些特殊功能键）
        if (Modifiers == ModifierKeys.None)
        {
            // 只有功能键可以不需要修饰键
            return Key >= VirtualKey.F1 && Key <= VirtualKey.F12;
        }

        // 主键不能是修饰键本身
        return Key switch
        {
            VirtualKey.Control or
            VirtualKey.Alt or
            VirtualKey.Shift or
            (VirtualKey)0x5B or // VK_LWIN
            (VirtualKey)0x5C => false, // VK_RWIN
            _ => true
        };
    }

    /// <summary>
    /// 创建一个新的热键组合
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <returns>热键组合实例</returns>
    public static HotkeyCombo Create(ModifierKeys modifiers, VirtualKey key)
    {
        return new HotkeyCombo(modifiers, key);
    }

    public static HotkeyCombo Create(uint p0, uint p1, uint p2)
    {
        ModifierKeys modifiers;
        VirtualKey key = default;
        switch ((VirtualKey)p0)
        {
            case VirtualKey.Control:
            case VirtualKey.LControl:
            case VirtualKey.RControl:
                modifiers = ModifierKeys.Control;
                break;
            case VirtualKey.Alt:
            case VirtualKey.LAlt:
            case VirtualKey.RAlt:
                modifiers = ModifierKeys.Alt;
                break;
            case VirtualKey.Shift:
            case VirtualKey.LShift:
            case VirtualKey.RShift:
                modifiers = ModifierKeys.Shift;
                break;
            case VirtualKey.LWin:
            case VirtualKey.RWin:
                modifiers = ModifierKeys.Win;
                break;
            default:
                modifiers = ModifierKeys.None;
                if (p1 == p2 && p2 == 0)
                {
                    return new HotkeyCombo(ModifierKeys.None, (VirtualKey)p0);
                }
                break;
        }
        switch ((VirtualKey)p1)
        {
            case VirtualKey.Control:
            case VirtualKey.LControl:
            case VirtualKey.RControl:
                modifiers |= ModifierKeys.Control;
                key = (VirtualKey)p2;
                break;
            case VirtualKey.Alt:
            case VirtualKey.LAlt:
            case VirtualKey.RAlt:
                modifiers |= ModifierKeys.Alt;
                key = (VirtualKey)p2;
                break;
            case VirtualKey.Shift:
            case VirtualKey.LShift:
            case VirtualKey.RShift:
                modifiers |= ModifierKeys.Shift;
                key = (VirtualKey)p2;
                break;
            case VirtualKey.LWin:
            case VirtualKey.RWin:
                modifiers |= ModifierKeys.Win;
                key = (VirtualKey)p2;
                break;
            default:
                if (p2 != 0)
                {
                    key = (VirtualKey)p2;
                }
                else
                {
                    key = (VirtualKey)p1;
                }
                break;
        }
        return new HotkeyCombo(modifiers, key);
    }

    /// <summary>
    /// 从字符串解析热键组合
    /// </summary>
    /// <param name="hotkeyString">热键字符串，格式如 "Ctrl + Alt + F1"</param>
    /// <param name="combo">解析出的热键组合</param>
    /// <returns>如果解析成功返回 true，否则返回 false</returns>
    public static bool TryParse(string hotkeyString, out HotkeyCombo combo)
    {
        combo = default;

        if (string.IsNullOrWhiteSpace(hotkeyString))
            return false;

        try
        {
            var parts = hotkeyString.Split(['+', ' '], StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .ToArray();

            if (parts.Length == 0)
                return false;

            var modifiers = ModifierKeys.None;
            VirtualKey key = default;
            bool hasMainKey = false;

            foreach (var part in parts)
            {
                switch (part.ToLowerInvariant())
                {
                    case "ctrl":
                    case "control":
                        modifiers |= ModifierKeys.Control;
                        break;
                    case "alt":
                        modifiers |= ModifierKeys.Alt;
                        break;
                    case "shift":
                        modifiers |= ModifierKeys.Shift;
                        break;
                    case "win":
                    case "windows":
                        modifiers |= ModifierKeys.Win;
                        break;
                    default:
                        if (!hasMainKey && TryParseKey(part, out key))
                        {
                            hasMainKey = true;
                        }
                        else
                        {
                            return false; // 无法解析的键或重复的主键
                        }
                        break;
                }
            }

            if (!hasMainKey)
                return false;

            combo = new HotkeyCombo(modifiers, key);
            return combo.IsValid();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试解析按键字符串
    /// </summary>
    static bool TryParseKey(string keyString, out VirtualKey key)
    {
        // 尝试直接解析枚举
        if (Enum.TryParse(keyString, true, out key))
            return true;

        // 处理特殊情况
        return keyString.ToLowerInvariant() switch
        {
            "space" => (key = VirtualKey.Space) != 0,
            "enter" => (key = VirtualKey.Enter) != 0,
            "esc" or "escape" => (key = VirtualKey.Escape) != 0,
            "tab" => (key = VirtualKey.Tab) != 0,
            _ when keyString.StartsWith("f", StringComparison.OrdinalIgnoreCase) &&
                   int.TryParse(keyString[1..], out var fNum) &&
                   fNum >= 1 && fNum <= 12 => (key = (VirtualKey)((int)VirtualKey.F1 + fNum - 1)) != 0,
            _ when keyString.StartsWith("numpad", StringComparison.OrdinalIgnoreCase) &&
                   keyString.Length > 6 &&
                   int.TryParse(keyString[6..].Trim(), out var numpadNum) &&
                   numpadNum >= 0 && numpadNum <= 9 => (key = (VirtualKey)((int)VirtualKey.NumPad0 + numpadNum)) != 0,
            _ when keyString.Length == 1 && char.IsDigit(keyString[0]) =>
                (key = (VirtualKey)((int)VirtualKey.D0 + (keyString[0] - '0'))) != 0,
            _ when keyString.Length == 1 && char.IsLetter(keyString[0]) =>
                (key = (VirtualKey)((int)VirtualKey.A + (char.ToUpperInvariant(keyString[0]) - 'A'))) != 0,
            _ => false
        };
    }

    ///// <summary>
    ///// 获取所有常用的热键组合示例
    ///// </summary>
    ///// <returns>常用热键组合列表</returns>
    //public static IReadOnlyList<HotkeyCombo> GetCommonCombos()
    //{
    //    return
    //    [
    //        new HotkeyCombo(ModifierKeys.Control | ModifierKeys.Alt, VirtualKey.F1),
    //        new HotkeyCombo(ModifierKeys.Control | ModifierKeys.Alt, VirtualKey.F2),
    //        new HotkeyCombo(ModifierKeys.Control | ModifierKeys.Shift, VirtualKey.Q),
    //        new HotkeyCombo(ModifierKeys.Win, VirtualKey.D1),
    //        new HotkeyCombo(ModifierKeys.Alt, VirtualKey.Space),
    //        new HotkeyCombo(ModifierKeys.None, VirtualKey.F1),
    //        new HotkeyCombo(ModifierKeys.None, VirtualKey.F2),
    //    ];
    //}

    /// <summary>
    /// 检查两个热键组合是否冲突（相同）
    /// </summary>
    /// <param name="other">另一个热键组合</param>
    /// <returns>如果冲突返回 true，否则返回 false</returns>
    public bool ConflictsWith(HotkeyCombo other)
    {
        return Modifiers == other.Modifiers && Key == other.Key;
    }
}
#endif