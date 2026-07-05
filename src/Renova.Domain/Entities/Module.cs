namespace Renova.Domain.Entities;

public class Module : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Permission> Permissions { get; set; } = [];

    public ICollection<MenuPermission> MenuPermissions { get; set; } = [];
}
