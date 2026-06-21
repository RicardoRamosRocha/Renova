using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Identity;

namespace Renova.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    public DbSet<Professional> Professionals => Set<Professional>();

    public DbSet<MedicalEvolution> MedicalEvolutions => Set<MedicalEvolution>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<StudentProgress> StudentProgress => Set<StudentProgress>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(user => user.IsActive)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>()
            .HasData(ApplicationRoles.Seed);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
