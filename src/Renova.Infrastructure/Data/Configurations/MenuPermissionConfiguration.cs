using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
{
    public void Configure(EntityTypeBuilder<MenuPermission> entity)
    {
        entity.ToTable("MenuPermissions");

        entity.HasKey(menuPermission => menuPermission.Id);

        entity.Property(menuPermission => menuPermission.CreatedAt)
            .IsRequired();

        entity.Property(menuPermission => menuPermission.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(menuPermission => !menuPermission.IsDeleted);

        entity.HasOne(menuPermission => menuPermission.Module)
            .WithMany(module => module.MenuPermissions)
            .HasForeignKey(menuPermission => menuPermission.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(menuPermission => menuPermission.Permission)
            .WithMany(permission => permission.MenuPermissions)
            .HasForeignKey(menuPermission => menuPermission.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(menuPermission => new { menuPermission.ModuleId, menuPermission.PermissionId })
            .IsUnique();
    }
}
