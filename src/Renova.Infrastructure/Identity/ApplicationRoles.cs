using Microsoft.AspNetCore.Identity;

namespace Renova.Infrastructure.Identity;

public static class ApplicationRoles
{
    public const string AdministratorAlias = "Administrator";
    public const string CoordinatorAlias = "Coordinator";
    public const string ProfessionalAlias = "Professional";
    public const string AttendantAlias = "Attendant";
    public const string TeacherAlias = "Teacher";
    public const string StudentAlias = "Student";
    public const string FamilyMemberAlias = "FamilyMember";

    public const string Administrator = "Administrador";
    public const string Coordinator = "Coordenador";
    public const string Professional = "Profissional";
    public const string Attendant = "Atendente";
    public const string Teacher = "Professor";
    public const string Student = "Aluno";
    public const string FamilyMember = "Familiar";

    public const string UserManagementRoles =
        AdministratorAlias + "," + Administrator;

    public const string StudentManagementRoles =
        AdministratorAlias + "," + Administrator + "," +
        CoordinatorAlias + "," + Coordinator + "," +
        AttendantAlias + "," + Attendant;

    public const string StudentRecordRoles =
        AdministratorAlias + "," + Administrator + "," +
        CoordinatorAlias + "," + Coordinator + "," +
        ProfessionalAlias + "," + Professional + "," +
        AttendantAlias + "," + Attendant;

    public const string ProfessionalManagementRoles =
        AdministratorAlias + "," + Administrator + "," +
        CoordinatorAlias + "," + Coordinator;

    public const string CourseManagementRoles =
        AdministratorAlias + "," + Administrator + "," +
        TeacherAlias + "," + Teacher;

    public const string FinancialManagementRoles =
        AdministratorAlias + "," + Administrator;

    public const string CourseAccessRoles =
        AdministratorAlias + "," + Administrator + "," +
        TeacherAlias + "," + Teacher + "," +
        StudentAlias + "," + Student + "," +
        FamilyMemberAlias + "," + FamilyMember;

    public static readonly string[] UserManagement =
    [
        AdministratorAlias,
        Administrator
    ];

    public static readonly string[] StudentManagement =
    [
        AdministratorAlias,
        Administrator,
        CoordinatorAlias,
        Coordinator,
        AttendantAlias,
        Attendant
    ];

    public static readonly string[] StudentRecords =
    [
        AdministratorAlias,
        Administrator,
        CoordinatorAlias,
        Coordinator,
        ProfessionalAlias,
        Professional,
        AttendantAlias,
        Attendant
    ];

    public static readonly string[] ProfessionalManagement =
    [
        AdministratorAlias,
        Administrator,
        CoordinatorAlias,
        Coordinator
    ];

    public static readonly string[] CourseManagement =
    [
        AdministratorAlias,
        Administrator,
        TeacherAlias,
        Teacher
    ];

    public static readonly string[] FinancialManagement =
    [
        AdministratorAlias,
        Administrator
    ];

    public static readonly string[] CourseAccess =
    [
        AdministratorAlias,
        Administrator,
        TeacherAlias,
        Teacher,
        StudentAlias,
        Student,
        FamilyMemberAlias,
        FamilyMember
    ];

    public static readonly string[] All =
    [
        Administrator,
        Coordinator,
        Professional,
        Attendant,
        Teacher,
        Student,
        FamilyMember
    ];

    public static readonly IdentityRole[] Seed =
    [
        Create("11111111-1111-1111-1111-111111111111", Administrator),
        Create("22222222-2222-2222-2222-222222222222", Coordinator),
        Create("33333333-3333-3333-3333-333333333333", Professional),
        Create("44444444-4444-4444-4444-444444444444", Attendant),
        Create("55555555-5555-5555-5555-555555555555", Teacher),
        Create("66666666-6666-6666-6666-666666666666", Student),
        Create("77777777-7777-7777-7777-777777777777", FamilyMember)
    ];

    private static IdentityRole Create(string id, string name)
    {
        return new IdentityRole
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            ConcurrencyStamp = id
        };
    }
}
