using Renova.Domain.Entities;

namespace Renova.Infrastructure.Identity;

public class UserPermission : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}
