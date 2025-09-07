using System.Text;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

static partial class ExceptionExtensions
{
    /// <summary>
    /// 获取异常中所有错误信息
    /// </summary>
    /// <param name="e">当前捕获的异常</param>
    /// <param name="msg">可选的消息，将写在第一行</param>
    /// <param name="args">可选的消息参数</param>
    /// <returns></returns>
    public static string GetAllMessage(this Exception e, string? msg = null, params object?[] args)
    {
        var has_msg = !string.IsNullOrWhiteSpace(msg);
        var has_args = args != null && args.Length != 0;
        StringBuilder sb = new();
        GetAllMessageCore(sb, e, has_msg, has_args, msg, args!);
        var text = sb.ToString().Trim();
        return text;
    }

    /// <summary>
    /// 追加异常中所有错误信息写入 <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="e">当前捕获的异常</param>
    /// <param name="sb"></param>
    /// <param name="msg">可选的消息，将写在第一行</param>
    /// <param name="args">可选的消息参数</param>
    public static void AppendAllMessage(this Exception? e, StringBuilder sb, string? msg = null, params object?[] args)
    {
        if (e == null)
            return;
        var has_msg = !string.IsNullOrWhiteSpace(msg);
        var has_args = args != null && args.Length != 0;
        GetAllMessageCore(sb, e, has_msg, has_args, msg, args!);
    }

    static void GetAllMessageCore(
        StringBuilder sb,
        Exception e,
        bool has_msg, bool has_args,
        string? msg = null, params object?[] args)
    {
        if (has_msg)
        {
            if (has_args)
            {
                try
                {
                    sb.AppendFormat(msg!, args);
                }
                catch
                {
                    sb.Append(msg);
                    foreach (var item in args)
                    {
                        sb.Append(' ');
                        sb.Append(item);
                    }
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine(msg!);
            }
        }

        var exception = e;
        ushort i = 0;
        while (exception != null && i++ < byte.MaxValue)
        {
            var exception_message = exception.Message;
            if (!string.IsNullOrWhiteSpace(exception_message))
            {
                sb.AppendLine(exception.ToString());
            }
            exception = exception.InnerException;
        }
    }
}