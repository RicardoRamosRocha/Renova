using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> entity)
    {
        entity.ToTable("TenantSettings");

        entity.HasKey(settings => settings.Id);

        entity.Property(settings => settings.LogoUrl)
            .HasMaxLength(500);

        entity.Property(settings => settings.PrimaryColor)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(settings => settings.SecondaryColor)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(settings => settings.CreatedAt)
            .IsRequired();

        entity.Property(settings => settings.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(settings => !settings.IsDeleted);

        entity.HasOne(settings => settings.Tenant)
            .WithOne(tenant => tenant.Settings)
            .HasForeignKey<TenantSettings>(settings => settings.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(settings => settings.TenantId)
            .IsUnique();
    }
}
