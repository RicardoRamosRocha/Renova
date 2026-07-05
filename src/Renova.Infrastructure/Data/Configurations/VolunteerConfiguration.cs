using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
{
    public void Configure(EntityTypeBuilder<Volunteer> entity)
    {
        entity.ToTable("Volunteers");

        entity.HasKey(volunteer => volunteer.Id);

        entity.Property(volunteer => volunteer.Area)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(volunteer => volunteer.Availability)
            .HasMaxLength(500);

        entity.Property(volunteer => volunteer.CreatedAt)
            .IsRequired();

        entity.Property(volunteer => volunteer.IsDeleted)
            .IsRequired();

        entity.HasOne(volunteer => volunteer.Person)
            .WithOne(person => person.Volunteer)
            .HasForeignKey<Volunteer>(volunteer => volunteer.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(volunteer => volunteer.Tenant)
            .WithMany()
            .HasForeignKey(volunteer => volunteer.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(volunteer => volunteer.PersonId)
            .IsUnique()
            .HasFilter("\"PersonId\" IS NOT NULL");

        entity.HasQueryFilter(volunteer => !volunteer.IsDeleted);
    }
}
