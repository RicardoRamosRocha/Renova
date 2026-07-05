using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("Permissions");

        entity.HasKey(permission => permission.Id);

        entity.Property(permission => permission.Name)
            .HasMaxLength(160)
            .IsRequired();

        entity.Property(permission => permission.Key)
            .HasMaxLength(160)
            .IsRequired();

        entity.Property(permission => permission.Description)
            .HasMaxLength(500);

        entity.Property(permission => permission.CreatedAt)
            .IsRequired();

        entity.Property(permission => permission.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(permission => !permission.IsDeleted);

        entity.HasOne(permission => permission.Module)
            .WithMany(module => module.Permissions)
            .HasForeignKey(permission => permission.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(permission => permission.Key)
            .IsUnique();

        entity.HasIndex(permission => permission.ModuleId);
    }
}
