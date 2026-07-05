using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> entity)
    {
        entity.ToTable("Tenants");

        entity.HasKey(tenant => tenant.Id);

        entity.Property(tenant => tenant.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(tenant => tenant.LegalName)
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(tenant => tenant.Cnpj)
            .HasMaxLength(18)
            .IsRequired();

        entity.Property(tenant => tenant.Email)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(tenant => tenant.Phone)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(tenant => tenant.Address)
            .HasMaxLength(500);

        entity.Property(tenant => tenant.City)
            .HasMaxLength(120);

        entity.Property(tenant => tenant.State)
            .HasMaxLength(2);

        entity.Property(tenant => tenant.ZipCode)
            .HasMaxLength(12);

        entity.Property(tenant => tenant.IsActive)
            .IsRequired();

        entity.Property(tenant => tenant.CreatedAt)
            .IsRequired();

        entity.Property(tenant => tenant.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(tenant => !tenant.IsDeleted);

        entity.HasIndex(tenant => tenant.Cnpj)
            .IsUnique();

        entity.HasIndex(tenant => tenant.Email);
    }
}
