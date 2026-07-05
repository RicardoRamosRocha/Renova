using Renova.Domain.Entities;

namespace Renova.Infrastructure.Identity;

public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;

    public ApplicationRole Role { get; set; } = null!;

    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}
