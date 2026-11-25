using Microsoft.AspNetCore.Identity;

namespace Clevora.Infrastructure.Identity;

public class ClevoraUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
