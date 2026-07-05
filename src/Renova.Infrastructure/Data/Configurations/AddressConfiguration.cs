using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> entity)
    {
        entity.ToTable("Addresses");

        entity.HasKey(address => address.Id);

        entity.Property(address => address.Street)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(address => address.Number)
            .HasMaxLength(30);

        entity.Property(address => address.Neighborhood)
            .HasMaxLength(120);

        entity.Property(address => address.City)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(address => address.State)
            .HasMaxLength(2)
            .IsRequired();

        entity.Property(address => address.ZipCode)
            .HasMaxLength(12)
            .IsRequired();

        entity.Property(address => address.Complement)
            .HasMaxLength(200);

        entity.Property(address => address.CreatedAt)
            .IsRequired();

        entity.Property(address => address.IsDeleted)
            .IsRequired();

        entity.HasOne(address => address.Person)
            .WithOne(person => person.Address)
            .HasForeignKey<Address>(address => address.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(address => address.Tenant)
            .WithMany()
            .HasForeignKey(address => address.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(address => address.PersonId)
            .IsUnique();

        entity.HasQueryFilter(address => !address.IsDeleted);
    }
}
