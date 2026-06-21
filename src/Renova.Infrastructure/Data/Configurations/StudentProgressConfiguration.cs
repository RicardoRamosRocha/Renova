using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class StudentProgressConfiguration : IEntityTypeConfiguration<StudentProgress>
{
    public void Configure(EntityTypeBuilder<StudentProgress> entity)
    {
        entity.ToTable("StudentProgress");

        entity.HasKey(progress => progress.Id);

        entity.Property(progress => progress.WatchedPercentage)
            .IsRequired();

        entity.Property(progress => progress.CreatedAt)
            .IsRequired();

        entity.HasOne(progress => progress.Student)
            .WithMany(student => student.ProgressEntries)
            .HasForeignKey(progress => progress.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(progress => progress.Lesson)
            .WithMany(lesson => lesson.ProgressEntries)
            .HasForeignKey(progress => progress.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(progress => new { progress.StudentId, progress.LessonId })
            .IsUnique();

        entity.HasIndex(progress => progress.LessonId);
    }
}
