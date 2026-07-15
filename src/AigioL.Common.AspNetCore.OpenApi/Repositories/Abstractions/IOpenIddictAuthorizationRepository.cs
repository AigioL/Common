namespace AigioL.Common.AspNetCore.OpenApi.Repositories.Abstractions;

public partial interface IOpenIddictAuthorizationRepository
{
    /// <summary>
    /// 根据 OpenId 查找 UserId
    /// </summary>
    Task<Guid?> GetUserIdByOpenIdAsync(Guid openId, CancellationToken cancellationToken = default);
}
