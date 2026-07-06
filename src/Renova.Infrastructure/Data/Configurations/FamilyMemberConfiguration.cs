using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> entity)
    {
        entity.ToTable("FamilyMembers");

        entity.HasKey(familyMember => familyMember.Id);

        entity.Property(familyMember => familyMember.TenantId)
            .IsRequired();

        entity.Ignore(familyMember => familyMember.Tenant);

        entity.Property(familyMember => familyMember.PersonId);

        entity.Property(familyMember => familyMember.FullName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(familyMember => familyMember.Relationship)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(familyMember => familyMember.Phone)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(familyMember => familyMember.Email)
            .HasMaxLength(200);

        entity.Property(familyMember => familyMember.CanAccessPortal)
            .IsRequired();

        entity.Property(familyMember => familyMember.IsResponsible)
            .IsRequired();

        entity.Property(familyMember => familyMember.CreatedAt)
            .IsRequired();

        entity.Property(familyMember => familyMember.IsDeleted)
            .IsRequired();

        entity.Ignore(familyMember => familyMember.DisplayName);
        entity.Ignore(familyMember => familyMember.DisplayEmail);
        entity.Ignore(familyMember => familyMember.DisplayPhone);
        entity.Ignore(familyMember => familyMember.DisplayPhotoUrl);

        entity.HasOne(familyMember => familyMember.Student)
            .WithMany(student => student.FamilyMembers)
            .HasForeignKey(familyMember => familyMember.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(familyMember => familyMember.Person)
            .WithMany(person => person.FamilyMembers)
            .HasForeignKey(familyMember => familyMember.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(familyMember => familyMember.StudentId);

        entity.HasIndex(familyMember => familyMember.PersonId)
            .HasFilter("\"PersonId\" IS NOT NULL");

        entity.HasQueryFilter(familyMember => !familyMember.IsDeleted);
    }
}
