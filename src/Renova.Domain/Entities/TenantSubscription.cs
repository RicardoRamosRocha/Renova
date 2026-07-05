namespace Renova.Domain.Entities;

public class TenantSubscription : BaseTenantEntity
{
    public Guid SubscriptionPlanId { get; set; }

    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
}
