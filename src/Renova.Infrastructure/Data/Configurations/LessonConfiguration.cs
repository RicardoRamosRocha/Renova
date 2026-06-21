using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> entity)
    {
        entity.ToTable("Lessons");

        entity.HasKey(lesson => lesson.Id);

        entity.Property(lesson => lesson.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(lesson => lesson.Description)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(lesson => lesson.VideoProvider)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(lesson => lesson.VideoExternalId)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(lesson => lesson.DurationInMinutes)
            .IsRequired();

        entity.Property(lesson => lesson.Order)
            .IsRequired();

        entity.Property(lesson => lesson.CreatedAt)
            .IsRequired();

        entity.HasOne(lesson => lesson.CourseModule)
            .WithMany(module => module.Lessons)
            .HasForeignKey(lesson => lesson.CourseModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(lesson => new { lesson.CourseModuleId, lesson.Order })
            .IsUnique();
    }
}
