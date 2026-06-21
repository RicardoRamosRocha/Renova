using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> entity)
    {
        entity.ToTable("MedicalRecords");

        entity.HasKey(medicalRecord => medicalRecord.Id);

        entity.Property(medicalRecord => medicalRecord.Anamnesis)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(medicalRecord => medicalRecord.ClinicalNotes)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(medicalRecord => medicalRecord.CreatedAt)
            .IsRequired();

        entity.HasOne(medicalRecord => medicalRecord.Student)
            .WithOne(student => student.MedicalRecord)
            .HasForeignKey<MedicalRecord>(medicalRecord => medicalRecord.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(medicalRecord => medicalRecord.StudentId)
            .IsUnique();
    }
}
