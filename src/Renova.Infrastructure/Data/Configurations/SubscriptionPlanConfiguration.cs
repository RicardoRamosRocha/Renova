using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> entity)
    {
        entity.ToTable("SubscriptionPlans");

        entity.HasKey(plan => plan.Id);

        entity.Property(plan => plan.Name)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(plan => plan.Description)
            .HasMaxLength(1000);

        entity.Property(plan => plan.MaxUsers)
            .IsRequired();

        entity.Property(plan => plan.MaxStudents)
            .IsRequired();

        entity.Property(plan => plan.MonthlyPrice)
            .HasPrecision(12, 2)
            .IsRequired();

        entity.Property(plan => plan.IsActive)
            .IsRequired();

        entity.Property(plan => plan.CreatedAt)
            .IsRequired();

        entity.Property(plan => plan.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(plan => !plan.IsDeleted);

        entity.HasIndex(plan => plan.Name)
            .IsUnique();
    }
}
