using Microsoft.AspNetCore.Identity;
#if !USE_NUM_UID
using UID = global::System.Guid;
#else
using UID = global::System.Int64;
#endif

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

public partial class Role : IdentityRole<UID>;

public partial class RoleClaim : IdentityRoleClaim<UID>;

public partial class UserClaim : IdentityUserClaim<UID>;

public partial class UserLogin : IdentityUserLogin<UID>;

public partial class UserRole : IdentityUserRole<UID>;

public partial class UserToken : IdentityUserToken<UID>;