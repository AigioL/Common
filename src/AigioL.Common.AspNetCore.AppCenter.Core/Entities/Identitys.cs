using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

public partial class Role : IdentityRole<Guid>
{
}

public partial class RoleClaim : IdentityRoleClaim<Guid>
{
}

public partial class UserClaim : IdentityUserClaim<Guid>
{
}

public partial class UserLogin : IdentityUserLogin<Guid>
{
}

public partial class UserRole : IdentityUserRole<Guid>
{
}

public partial class UserToken : IdentityUserToken<Guid>
{
}