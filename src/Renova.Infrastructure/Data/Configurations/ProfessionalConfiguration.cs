using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> entity)
    {
        entity.ToTable("Professionals");

        entity.HasKey(professional => professional.Id);

        entity.Property(professional => professional.TenantId)
            .IsRequired();

        entity.Ignore(professional => professional.Tenant);

        entity.Property(professional => professional.PersonId);

        entity.Property(professional => professional.FullName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(professional => professional.Specialty)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(professional => professional.RegistrationNumber)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(professional => professional.Email)
            .HasMaxLength(200);

        entity.Property(professional => professional.Phone)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(professional => professional.IsActive)
            .IsRequired();

        entity.Property(professional => professional.CreatedAt)
            .IsRequired();

        entity.Property(professional => professional.IsDeleted)
            .IsRequired();

        entity.Ignore(professional => professional.DisplayName);
        entity.Ignore(professional => professional.DisplayEmail);
        entity.Ignore(professional => professional.DisplayPhone);
        entity.Ignore(professional => professional.DisplayPhotoUrl);

        entity.HasOne(professional => professional.Person)
            .WithOne(person => person.Professional)
            .HasForeignKey<Professional>(professional => professional.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(professional => new { professional.TenantId, professional.RegistrationNumber })
            .IsUnique()
            .HasFilter("\"RegistrationNumber\" IS NOT NULL AND \"IsDeleted\" = false");

        entity.HasIndex(professional => professional.PersonId)
            .IsUnique()
            .HasFilter("\"PersonId\" IS NOT NULL");

        entity.HasQueryFilter(professional => !professional.IsDeleted);
    }
}
