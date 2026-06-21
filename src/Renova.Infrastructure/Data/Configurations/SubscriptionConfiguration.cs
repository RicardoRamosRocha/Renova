using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> entity)
    {
        entity.ToTable("Subscriptions");

        entity.HasKey(subscription => subscription.Id);

        entity.Property(subscription => subscription.PlanName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(subscription => subscription.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        entity.Property(subscription => subscription.Status)
            .IsRequired();

        entity.Property(subscription => subscription.NextDueDate)
            .IsRequired();

        entity.Property(subscription => subscription.CreatedAt)
            .IsRequired();

        entity.HasOne(subscription => subscription.Student)
            .WithMany(student => student.Subscriptions)
            .HasForeignKey(subscription => subscription.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(subscription => subscription.StudentId);
    }
}
