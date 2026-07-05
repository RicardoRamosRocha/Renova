using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Infrastructure.Identity;

namespace Renova.Infrastructure.Data.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> entity)
    {
        entity.ToTable("UserPermissions");

        entity.HasKey(userPermission => userPermission.Id);

        entity.Property(userPermission => userPermission.UserId)
            .HasMaxLength(450)
            .IsRequired();

        entity.Property(userPermission => userPermission.CreatedAt)
            .IsRequired();

        entity.Property(userPermission => userPermission.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(userPermission => !userPermission.IsDeleted);

        entity.HasOne(userPermission => userPermission.User)
            .WithMany()
            .HasForeignKey(userPermission => userPermission.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(userPermission => userPermission.Permission)
            .WithMany()
            .HasForeignKey(userPermission => userPermission.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(userPermission => new { userPermission.UserId, userPermission.PermissionId })
            .IsUnique();
    }
}
