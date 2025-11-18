using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Shared;
using AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties;
using AigioL.Common.AspNetCore.AppCenter.Identity.Views;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

/// <summary>
/// 登录检测页面模型类
/// </summary>
public sealed partial record class LoginDetectionModel
{
    /// <summary>
    /// 将模型类转换为 <see cref="IResult"/>
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public IResult ToResult(int statusCode = StatusCodes.Status200OK)
    {
        var result = new LoginDetection(this, statusCode: statusCode);
        return result;
    }

    /// <inheritdoc cref="LayoutModel"/>
    public required LayoutModel Layout
    {
        get
        {
            if (string.IsNullOrWhiteSpace(field.HeadTitle))
            {
                field.SetSubHeadTitle(Resources.LoginAndRegister);
            }
            return field;
        }
        init => field = value;
    }

    /// <summary>
    /// 登录成功的 JWT 值
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// 唤起客户端应用程序的 HTTP 端口号
    /// </summary>
    public string? Port { get; init; }

    /// <summary>
    /// 发生错误时的显示文本
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// 第三方登录渠道枚举名称
    /// </summary>
    public string? Channel { get; init; }

    /// <summary>
    /// 第三方登录渠道枚举整数值
    /// </summary>
    public int ChannelInt32 { get; init; }

    /// <summary>
    /// 是否使用 UrlScheme 协议唤起客户端应用程序
    /// </summary>
    public bool UseUrlSchemeLoginToken { get; init; }

    /// <summary>
    /// 请稍后…
    /// </summary>
    public string PleaseWait
    {
        get => field ?? Resources.PleaseWait;
        init;
    }

    /// <summary>
    /// 很抱歉，您正在使用一个过时的浏览器。建议升级您的浏览器或使用
    /// </summary>
    public string BrowserIsSupported1
    {
        get => field ?? Resources.BrowserIsSupported1;
        init;
    }

    /// <summary>
    /// 等其他现代浏览器，以提高您的体验。
    /// </summary>
    public string BrowserIsSupported2
    {
        get => field ?? Resources.BrowserIsSupported2;
        init;
    }

    public string Url_MicrosoftEdge
    {
        get => field ?? Resources.Url_MicrosoftEdge;
        init;
    }

    public string Url_Chrome
    {
        get => field ?? Resources.Url_Chrome;
        init;
    }

    public string Url_Firefox
    {
        get => field ?? Resources.Url_Firefox;
        init;
    }

    /// <summary>
    /// 错误!
    /// </summary>
    public string ModelErrorTitle
    {
        get => field ?? Resources.ModelErrorTitle;
        init;
    }

    public string ModelSuccessTitle
    {
        get => field ?? string.Format(ModelSuccessTitle_, LoginOrBindText);
        init;
    }

    /// <summary>
    /// {0}成功!
    /// </summary>
    public string ModelSuccessTitle_
    {
        get => field ?? Resources.ModelSuccessTitle_;
        init;
    }

    /// <summary>
    /// 登录地址（相对路径）
    /// </summary>
    public string? LoginUrl { get; init; }

    /// <summary>
    /// 成功复制到剪贴板
    /// </summary>
    public string CopySuccess
    {
        get => field ?? Resources.CopySuccess;
        init;
    }

    /// <summary>
    /// 该浏览器不支持点击复制到剪贴板
    /// </summary>
    public string CopyNoSupport
    {
        get => field ?? Resources.CopyNoSupport;
        init;
    }

    /// <summary>
    /// 传递登录成功的 JWT 值的 UrlScheme 协议，例如：{xyz}://login/{token}
    /// </summary>
    public string? UrlSchemeLoginToken { get; init; }

    /// <summary>
    /// 是否为绑定第三方登录渠道
    /// </summary>
    public bool IsBind { get; init; }

    /// <summary>
    /// var loginOrBindText = isBind ? 绑定 : 登录;
    /// </summary>
    public string LoginOrBindText
    {
        get => field ?? (IsBind ? Resources.Bind : Resources.Login2);
        set;
    }

    /// <summary>
    /// Resources.LoginSuccessTip1.Format(channel, loginOrBindText)
    /// </summary>
    public string LoginSuccessTip1
    {
        get => field ?? string.Format(LoginSuccessTip1___, Channel, LoginOrBindText, Layout.AppName);
        init;
    }

    /// <summary>
    /// {0} {1}已完成，您可以关闭此窗口并返回至 {2}。
    /// </summary>
    public string LoginSuccessTip1___
    {
        get => field ?? Resources.LoginSuccessTip1___;
        init;
    }

    /// <summary>
    /// 此浏览器窗口/标签页将会在 10 秒内尝试自动关闭，您也可以手动关闭。
    /// </summary>
    public string LoginSuccessTip2
    {
        get => field ?? Resources.LoginSuccessTip2;
        set;
    }

    /// <summary>
    /// 点击此处重试
    /// </summary>
    public string ClickHereTryAgain
    {
        get => field ?? Resources.ClickHereTryAgain;
        set;
    }

    /// <summary>
    /// 与程序的连接丢失，点击重试或重新在程序内点击快速登录。
    /// </summary>
    public string WebSocketLostTip
    {
        get => field ?? Resources.WebSocketLostTip;
        set;
    }
    /// <summary>
    /// 请检查是否能正常访问 Steam 社区。
    /// </summary>
    public string PleaseCheckSteamCommunity
    {
        get => field ?? Resources.PleaseCheckSteamCommunity;
        set;
    }

    /// <summary>
    /// 长时间未跳转？
    /// </summary>
    public string LongTimeNoJump
    {
        get => field ?? Resources.LongTimeNoJump;
        set;
    }

    /// <summary>
    /// 连接程序出错，请手动复制以下内容到程序点击手动登录。
    /// </summary>
    public string ManualCopyTip
    {
        get => field ?? Resources.ManualCopyTip;
        set;
    }

    /// <summary>
    /// 点此复制
    /// </summary>
    public string CopyButton
    {
        get => field ?? Resources.CopyButton;
        set;
    }
}