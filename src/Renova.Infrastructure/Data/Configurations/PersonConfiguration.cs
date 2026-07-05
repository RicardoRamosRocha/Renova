using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> entity)
    {
        entity.ToTable("People");

        entity.HasKey(person => person.Id);

        entity.Property(person => person.RegistrationNumber)
            .HasMaxLength(50);

        entity.Property(person => person.FullName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(person => person.SocialName)
            .HasMaxLength(200);

        entity.Property(person => person.Cpf)
            .HasMaxLength(14);

        entity.Property(person => person.Rg)
            .HasMaxLength(30);

        entity.Property(person => person.Nationality)
            .HasMaxLength(80);

        entity.Property(person => person.BirthPlace)
            .HasMaxLength(120);

        entity.Property(person => person.Occupation)
            .HasMaxLength(120);

        entity.Property(person => person.Email)
            .HasMaxLength(200);

        entity.Property(person => person.Phone)
            .HasMaxLength(20);

        entity.Property(person => person.Mobile)
            .HasMaxLength(20);

        entity.Property(person => person.Whatsapp)
            .HasMaxLength(20);

        entity.Property(person => person.PhotoUrl)
            .HasMaxLength(500);

        entity.Property(person => person.Notes)
            .HasMaxLength(2000);

        entity.Property(person => person.IsActive)
            .IsRequired();

        entity.Property(person => person.CreatedAt)
            .IsRequired();

        entity.Property(person => person.IsDeleted)
            .IsRequired();

        entity.HasOne(person => person.Tenant)
            .WithMany()
            .HasForeignKey(person => person.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(person => new { person.TenantId, person.Cpf })
            .IsUnique()
            .HasFilter("\"Cpf\" IS NOT NULL AND \"IsDeleted\" = false");

        entity.HasIndex(person => new { person.TenantId, person.RegistrationNumber })
            .IsUnique()
            .HasFilter("\"RegistrationNumber\" IS NOT NULL AND \"IsDeleted\" = false");

        entity.HasIndex(person => new { person.TenantId, person.FullName });

        entity.HasQueryFilter(person => !person.IsDeleted);
    }
}
