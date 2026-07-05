using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> entity)
    {
        entity.ToTable("Modules");

        entity.HasKey(module => module.Id);

        entity.Property(module => module.Name)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(module => module.Key)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(module => module.Description)
            .HasMaxLength(500);

        entity.Property(module => module.Icon)
            .HasMaxLength(80);

        entity.Property(module => module.DisplayOrder)
            .IsRequired();

        entity.Property(module => module.IsActive)
            .IsRequired();

        entity.Property(module => module.CreatedAt)
            .IsRequired();

        entity.Property(module => module.IsDeleted)
            .IsRequired();

        entity.HasQueryFilter(module => !module.IsDeleted);

        entity.HasIndex(module => module.Key)
            .IsUnique();
    }
}
