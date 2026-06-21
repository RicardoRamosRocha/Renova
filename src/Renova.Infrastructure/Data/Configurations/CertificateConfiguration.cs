using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> entity)
    {
        entity.ToTable("Certificates");

        entity.HasKey(certificate => certificate.Id);

        entity.Property(certificate => certificate.VerificationCode)
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(certificate => certificate.IssuedAt)
            .IsRequired();

        entity.HasOne(certificate => certificate.Student)
            .WithMany(student => student.Certificates)
            .HasForeignKey(certificate => certificate.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(certificate => certificate.Course)
            .WithMany(course => course.Certificates)
            .HasForeignKey(certificate => certificate.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(certificate => new { certificate.StudentId, certificate.CourseId })
            .IsUnique();

        entity.HasIndex(certificate => certificate.VerificationCode)
            .IsUnique();
    }
}
