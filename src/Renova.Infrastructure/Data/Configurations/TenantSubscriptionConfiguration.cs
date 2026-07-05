using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> entity)
    {
        entity.ToTable("TenantSubscriptions");

        entity.HasKey(subscription => subscription.Id);

        entity.Property(subscription => subscription.StartDate)
            .IsRequired();

        entity.Property(subscription => subscription.IsActive)
            .IsRequired();

        entity.Property(subscription => subscription.Status)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(subscription => subscription.CreatedAt)
            .IsRequired();

        entity.Property(subscription => subscription.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(subscription => !subscription.IsDeleted);

        entity.HasOne(subscription => subscription.Tenant)
            .WithMany(tenant => tenant.Subscriptions)
            .HasForeignKey(subscription => subscription.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(subscription => subscription.SubscriptionPlan)
            .WithMany(plan => plan.TenantSubscriptions)
            .HasForeignKey(subscription => subscription.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(subscription => subscription.TenantId);

        entity.HasIndex(subscription => subscription.SubscriptionPlanId);
    }
}
