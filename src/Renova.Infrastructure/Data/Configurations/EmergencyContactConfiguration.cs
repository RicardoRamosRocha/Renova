using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> entity)
    {
        entity.ToTable("EmergencyContacts");

        entity.HasKey(contact => contact.Id);

        entity.Property(contact => contact.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(contact => contact.Phone)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(contact => contact.Whatsapp)
            .HasMaxLength(20);

        entity.Property(contact => contact.Email)
            .HasMaxLength(200);

        entity.Property(contact => contact.Notes)
            .HasMaxLength(1000);

        entity.Property(contact => contact.CreatedAt)
            .IsRequired();

        entity.Property(contact => contact.IsDeleted)
            .IsRequired();

        entity.HasOne(contact => contact.Person)
            .WithMany(person => person.EmergencyContacts)
            .HasForeignKey(contact => contact.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(contact => contact.Tenant)
            .WithMany()
            .HasForeignKey(contact => contact.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(contact => contact.PersonId);

        entity.HasQueryFilter(contact => !contact.IsDeleted);
    }
}
