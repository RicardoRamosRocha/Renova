using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Identity;

namespace Renova.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Person> People => Set<Person>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Admission> Admissions => Set<Admission>();

    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    public DbSet<Professional> Professionals => Set<Professional>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Volunteer> Volunteers => Set<Volunteer>();

    public DbSet<MedicalEvolution> MedicalEvolutions => Set<MedicalEvolution>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<StudentProgress> StudentProgress => Set<StudentProgress>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    public DbSet<Module> Modules => Set<Module>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    public DbSet<MenuPermission> MenuPermissions => Set<MenuPermission>();

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

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(role => role.Description)
                .HasMaxLength(500);

            entity.Property(role => role.IsSystemRole)
                .IsRequired();

            entity.Property(role => role.CreatedAt)
                .IsRequired();

            entity.Property(role => role.IsDeleted)
                .IsRequired();

            entity.HasQueryFilter(role => !role.IsDeleted);

            entity.HasData(ApplicationRoles.Seed);
        });

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
