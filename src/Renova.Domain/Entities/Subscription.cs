namespace Renova.Domain.Entities;

public class Subscription
{
    // TODO Sprint 4: evaluate BaseTenantEntity adoption after a migration plan adds TenantId and IsDeleted.
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>
    /// LEGACY: numeric status kept for compatibility. Prefer SubscriptionStatus for new flows.
    /// </summary>
    public int Status { get; set; }

    public DateTime NextDueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
}
