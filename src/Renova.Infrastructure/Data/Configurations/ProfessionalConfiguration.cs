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

        entity.HasIndex(professional => professional.RegistrationNumber)
            .IsUnique();
    }
}
