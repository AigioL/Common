namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 短信验证码
/// </summary>
public interface ISmsCode : IReadOnlySmsCode
{
    new string? SmsCode { get; set; }
}

/// <inheritdoc cref="ISmsCode"/>
public interface IReadOnlySmsCode
{
    /// <inheritdoc cref="ISmsCode"/>
    string? SmsCode { get; }
}
