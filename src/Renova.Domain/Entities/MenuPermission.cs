namespace Renova.Domain.Entities;

public class MenuPermission : BaseEntity
{
    public Guid ModuleId { get; set; }

    public Module Module { get; set; } = null!;

    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}
