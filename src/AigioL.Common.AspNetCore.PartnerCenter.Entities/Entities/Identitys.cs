using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <inheritdoc cref="IdentityRoleClaim{TKey}"/>
public partial class PCRoleClaim : IdentityRoleClaim<Guid>
{
}

/// <inheritdoc cref="IdentityUserClaim{TKey}"/>
public partial class PCUserClaim : IdentityUserClaim<Guid>
{
}

/// <inheritdoc cref="IdentityUserLogin{TKey}"/>
public partial class PCUserLogin : IdentityUserLogin<Guid>
{
}

/// <inheritdoc cref="IdentityUserToken{TKey}"/>
public partial class PCUserToken : IdentityUserToken<Guid>
{
}