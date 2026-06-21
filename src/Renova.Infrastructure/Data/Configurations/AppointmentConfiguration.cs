using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> entity)
    {
        entity.ToTable("Appointments");

        entity.HasKey(appointment => appointment.Id);

        entity.Property(appointment => appointment.ScheduledAt)
            .IsRequired();

        entity.Property(appointment => appointment.Status)
            .IsRequired();

        entity.Property(appointment => appointment.Notes)
            .HasColumnType("text");

        entity.Property(appointment => appointment.CreatedAt)
            .IsRequired();

        entity.HasOne(appointment => appointment.Student)
            .WithMany(student => student.Appointments)
            .HasForeignKey(appointment => appointment.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(appointment => appointment.Professional)
            .WithMany(professional => professional.Appointments)
            .HasForeignKey(appointment => appointment.ProfessionalId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(appointment => appointment.StudentId);

        entity.HasIndex(appointment => appointment.ProfessionalId);

        entity.HasIndex(appointment => appointment.ScheduledAt);
    }
}
