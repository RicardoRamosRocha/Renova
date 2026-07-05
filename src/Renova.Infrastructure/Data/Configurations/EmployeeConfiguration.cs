using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.ToTable("Employees");

        entity.HasKey(employee => employee.Id);

        entity.Property(employee => employee.Department)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(employee => employee.Position)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(employee => employee.AdmissionDate)
            .IsRequired();

        entity.Property(employee => employee.CreatedAt)
            .IsRequired();

        entity.Property(employee => employee.IsDeleted)
            .IsRequired();

        entity.HasOne(employee => employee.Person)
            .WithOne(person => person.Employee)
            .HasForeignKey<Employee>(employee => employee.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(employee => employee.Tenant)
            .WithMany()
            .HasForeignKey(employee => employee.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(employee => employee.PersonId)
            .IsUnique()
            .HasFilter("\"PersonId\" IS NOT NULL");

        entity.HasQueryFilter(employee => !employee.IsDeleted);
    }
}
