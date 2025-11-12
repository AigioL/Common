using MemoryPack;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

[MemoryPackable]
public sealed partial record UserDeviceIsTrustWithUserId(Guid UserId, bool IsTrust)
{
}

[MemoryPackable]
public sealed partial record UserDeviceIsTrustWithId(Guid Id, bool IsTrust)
{
}

#if DEBUG
[Obsolete("use UserDeviceIsTrustWithUserId", true)]
public partial record UserJsonWebTokenInfo(Guid UserId, bool IsTrust);
#endif