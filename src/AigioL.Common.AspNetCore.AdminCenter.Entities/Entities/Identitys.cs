using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <inheritdoc cref="IdentityRoleClaim{TKey}"/>
public partial class BMRoleClaim : IdentityRoleClaim<Guid>
{
}

/// <inheritdoc cref="IdentityUserClaim{TKey}"/>
public partial class BMUserClaim : IdentityUserClaim<Guid>
{
}

/// <inheritdoc cref="IdentityUserLogin{TKey}"/>
public partial class BMUserLogin : IdentityUserLogin<Guid>
{
}

/// <inheritdoc cref="IdentityUserToken{TKey}"/>
public partial class BMUserToken : IdentityUserToken<Guid>
{
}