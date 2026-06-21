using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        entity.ToTable("Payments");

        entity.HasKey(payment => payment.Id);

        entity.Property(payment => payment.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        entity.Property(payment => payment.Status)
            .IsRequired();

        entity.Property(payment => payment.PaymentMethod)
            .IsRequired();

        entity.Property(payment => payment.ExternalPaymentId)
            .HasMaxLength(200);

        entity.Property(payment => payment.CreatedAt)
            .IsRequired();

        entity.HasOne(payment => payment.Student)
            .WithMany(student => student.Payments)
            .HasForeignKey(payment => payment.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(payment => payment.StudentId);

        entity.HasIndex(payment => payment.ExternalPaymentId)
            .IsUnique();
    }
}
