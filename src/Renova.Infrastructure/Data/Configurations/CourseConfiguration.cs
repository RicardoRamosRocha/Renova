using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> entity)
    {
        entity.ToTable("Courses");

        entity.HasKey(course => course.Id);

        entity.Property(course => course.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(course => course.Description)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(course => course.IsActive)
            .IsRequired();

        entity.Property(course => course.CreatedAt)
            .IsRequired();
    }
}
