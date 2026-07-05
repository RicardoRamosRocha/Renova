using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> entity)
    {
        entity.ToTable("Students");

        entity.HasKey(student => student.Id);

        entity.Property(student => student.TenantId)
            .IsRequired();

        entity.Ignore(student => student.Tenant);

        entity.Property(student => student.PersonId);

        entity.Property(student => student.FullName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(student => student.CPF)
            .HasMaxLength(14)
            .IsRequired();

        entity.Property(student => student.Phone)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(student => student.Email)
            .HasMaxLength(200);

        entity.Property(student => student.Address)
            .HasMaxLength(500);

        entity.Property(student => student.Status)
            .IsRequired();

        entity.Property(student => student.BirthDate)
            .IsRequired();

        entity.Property(student => student.AdmissionDate)
            .IsRequired();

        entity.Property(student => student.AllergyDescription)
            .HasMaxLength(1000);

        entity.Property(student => student.MedicationDescription)
            .HasMaxLength(1000);

        entity.Property(student => student.DisabilityDescription)
            .HasMaxLength(1000);

        entity.Property(student => student.Observation)
            .HasMaxLength(2000);

        entity.Property(student => student.CreatedAt)
            .IsRequired();

        entity.Property(student => student.IsDeleted)
            .IsRequired();

        entity.HasOne(student => student.Person)
            .WithOne(person => person.Student)
            .HasForeignKey<Student>(student => student.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(student => new { student.TenantId, student.CPF })
            .IsUnique()
            .HasFilter("\"CPF\" IS NOT NULL AND \"IsDeleted\" = false");

        entity.HasIndex(student => student.PersonId)
            .IsUnique()
            .HasFilter("\"PersonId\" IS NOT NULL");
    }
}
