namespace Renova.Domain.Entities;

public class TenantSettings : BaseTenantEntity
{
    public string? LogoUrl { get; set; }

    public string PrimaryColor { get; set; } = "#2563EB";

    public string SecondaryColor { get; set; } = "#10B981";

    public bool AllowFamilyPortal { get; set; }

    public bool AllowEad { get; set; }

    public bool AllowInventory { get; set; }

    public bool AllowTherapeuticPlans { get; set; }
}
