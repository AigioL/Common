using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

public partial class BMRoleClaim : IdentityRoleClaim<Guid>
{
}

public partial class BMUserClaim : IdentityUserClaim<Guid>
{
}

public partial class BMUserLogin : IdentityUserLogin<Guid>
{
}

public partial class BMUserToken : IdentityUserToken<Guid>
{
}