using MemoryPack;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

[MemoryPackable]
public sealed partial record UserDeviceIsTrustWithUserId(
#if !USE_NUM_UID
    Guid UserId,
#else
    long UserId,
#endif
    bool IsTrust)
{
}

[MemoryPackable]
public sealed partial record UserDeviceIsTrustWithId(
    Guid Id,
    bool IsTrust)
{
}

#if DEBUG
[Obsolete("use UserDeviceIsTrustWithUserId", true)]
public partial record UserJsonWebTokenInfo(
#if !USE_NUM_UID
    Guid UserId,
#else
    long UserId,
#endif
    bool IsTrust);
#endif