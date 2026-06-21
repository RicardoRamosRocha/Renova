using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class CourseModuleConfiguration : IEntityTypeConfiguration<CourseModule>
{
    public void Configure(EntityTypeBuilder<CourseModule> entity)
    {
        entity.ToTable("CourseModules");

        entity.HasKey(module => module.Id);

        entity.Property(module => module.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(module => module.Description)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(module => module.Order)
            .IsRequired();

        entity.Property(module => module.CreatedAt)
            .IsRequired();

        entity.HasOne(module => module.Course)
            .WithMany(course => course.Modules)
            .HasForeignKey(module => module.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(module => new { module.CourseId, module.Order })
            .IsUnique();
    }
}
