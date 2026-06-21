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

        entity.Property(familyMember => familyMember.CreatedAt)
            .IsRequired();

        entity.HasOne(familyMember => familyMember.Student)
            .WithMany(student => student.FamilyMembers)
            .HasForeignKey(familyMember => familyMember.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(familyMember => familyMember.StudentId);
    }
}
