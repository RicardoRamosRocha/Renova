using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Infrastructure.Identity;

namespace Renova.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("RolePermissions");

        entity.HasKey(rolePermission => rolePermission.Id);

        entity.Property(rolePermission => rolePermission.RoleId)
            .HasMaxLength(450)
            .IsRequired();

        entity.Property(rolePermission => rolePermission.CreatedAt)
            .IsRequired();

        entity.Property(rolePermission => rolePermission.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(rolePermission => !rolePermission.IsDeleted);

        entity.HasOne(rolePermission => rolePermission.Role)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(rolePermission => rolePermission.Permission)
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .IsUnique();
    }
}
