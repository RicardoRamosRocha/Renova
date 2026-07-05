namespace Renova.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int MaxUsers { get; set; }

    public int MaxStudents { get; set; }

    public bool HasEad { get; set; }

    public bool HasFinance { get; set; }

    public bool HasReports { get; set; }

    public bool HasFamilyPortal { get; set; }

    public bool HasInventory { get; set; }

    public bool HasTherapeuticPlans { get; set; }

    public decimal MonthlyPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = [];
}
