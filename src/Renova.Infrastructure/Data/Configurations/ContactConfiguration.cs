using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> entity)
    {
        entity.ToTable("Contacts");

        entity.HasKey(contact => contact.Id);

        entity.Property(contact => contact.Value)
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(contact => contact.Observation)
            .HasMaxLength(500);

        entity.Property(contact => contact.CreatedAt)
            .IsRequired();

        entity.Property(contact => contact.IsDeleted)
            .IsRequired();

        entity.HasOne(contact => contact.Person)
            .WithMany(person => person.Contacts)
            .HasForeignKey(contact => contact.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(contact => contact.Tenant)
            .WithMany()
            .HasForeignKey(contact => contact.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(contact => new { contact.PersonId, contact.ContactType });

        entity.HasQueryFilter(contact => !contact.IsDeleted);
    }
}
