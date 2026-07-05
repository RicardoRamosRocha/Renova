using Microsoft.AspNetCore.Identity;

namespace Renova.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }

    public bool IsSystemRole { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
