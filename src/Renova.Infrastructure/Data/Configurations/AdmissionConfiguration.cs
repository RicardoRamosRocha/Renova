using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> entity)
    {
        entity.ToTable("Admissions");

        entity.HasKey(admission => admission.Id);

        entity.Property(admission => admission.AdmissionDate)
            .IsRequired();

        entity.Property(admission => admission.AdmissionReason)
            .HasMaxLength(1000);

        entity.Property(admission => admission.DischargeReason)
            .HasMaxLength(1000);

        entity.Property(admission => admission.ReferredBy)
            .HasMaxLength(200);

        entity.Property(admission => admission.Notes)
            .HasMaxLength(2000);

        entity.Property(admission => admission.CreatedAt)
            .IsRequired();

        entity.Property(admission => admission.IsDeleted)
            .IsRequired();

        entity.HasOne(admission => admission.Student)
            .WithMany(student => student.Admissions)
            .HasForeignKey(admission => admission.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(admission => admission.Tenant)
            .WithMany()
            .HasForeignKey(admission => admission.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(admission => new { admission.TenantId, admission.StudentId, admission.AdmissionDate });

        entity.HasQueryFilter(admission => !admission.IsDeleted);
    }
}
