namespace Renova.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid ModuleId { get; set; }

    public Module Module { get; set; } = null!;

    public ICollection<MenuPermission> MenuPermissions { get; set; } = [];
}
