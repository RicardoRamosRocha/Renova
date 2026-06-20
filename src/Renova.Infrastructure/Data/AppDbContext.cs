using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");

            entity.HasKey(student => student.Id);

            entity.Property(student => student.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(student => student.CPF)
                .HasMaxLength(14)
                .IsRequired();

            entity.Property(student => student.Phone)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(student => student.Email)
                .HasMaxLength(200);

            entity.Property(student => student.Address)
                .HasMaxLength(500);

            entity.Property(student => student.Status)
                .IsRequired();

            entity.Property(student => student.BirthDate)
                .IsRequired();

            entity.Property(student => student.AdmissionDate)
                .IsRequired();

            entity.Property(student => student.CreatedAt)
                .IsRequired();

            entity.HasIndex(student => student.CPF)
                .IsUnique();
        });
    }
}
