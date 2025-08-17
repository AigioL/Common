namespace AigioL.Common.AspNetCore.AdminCenter.Models;

public sealed class BMUserTableItem
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? NickName { get; set; }

    public List<string> Roles { get; set; } = null!;

    public bool LockoutEnabled { get; set; }
}
