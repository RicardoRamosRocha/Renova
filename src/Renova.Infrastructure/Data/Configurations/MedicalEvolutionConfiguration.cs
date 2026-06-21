using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class MedicalEvolutionConfiguration : IEntityTypeConfiguration<MedicalEvolution>
{
    public void Configure(EntityTypeBuilder<MedicalEvolution> entity)
    {
        entity.ToTable("MedicalEvolutions");

        entity.HasKey(medicalEvolution => medicalEvolution.Id);

        entity.Property(medicalEvolution => medicalEvolution.Description)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(medicalEvolution => medicalEvolution.CreatedAt)
            .IsRequired();

        entity.Property(medicalEvolution => medicalEvolution.IsDeleted)
            .IsRequired();

        entity.HasOne(medicalEvolution => medicalEvolution.Student)
            .WithMany(student => student.MedicalEvolutions)
            .HasForeignKey(medicalEvolution => medicalEvolution.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(medicalEvolution => medicalEvolution.Professional)
            .WithMany(professional => professional.MedicalEvolutions)
            .HasForeignKey(medicalEvolution => medicalEvolution.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(medicalEvolution => medicalEvolution.StudentId);

        entity.HasIndex(medicalEvolution => medicalEvolution.ProfessionalId);
    }
}
