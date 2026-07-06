using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AllergyDescription)
            .HasMaxLength(500);

        builder.Property(x => x.MedicationDescription)
            .HasMaxLength(500);

        builder.Property(x => x.DisabilityDescription)
            .HasMaxLength(500);

        builder.Property(x => x.Observation)
            .HasMaxLength(1000);

        builder.Ignore(x => x.DisplayName);
        builder.Ignore(x => x.DisplayCpf);
        builder.Ignore(x => x.DisplayEmail);
        builder.Ignore(x => x.DisplayPhone);
        builder.Ignore(x => x.DisplayBirthDate);
        builder.Ignore(x => x.DisplayPhotoUrl);

        builder.HasOne(x => x.Person)
            .WithOne(x => x.Student)
            .HasForeignKey<Student>(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.PersonId })
            .IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
