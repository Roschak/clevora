using Microsoft.AspNetCore.Identity;

namespace Clevora.Infrastructure.Identity;

public class ClevoraUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    /// <summary>null = currently online; set to UtcNow when disconnecting</summary>
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
