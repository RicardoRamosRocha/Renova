namespace Renova.Infrastructure.Identity;

public static class ApplicationRoles
{
    public const string SuperAdmin = "SuperAdmin";
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
    public const string Family = FamilyMember;

    public const string UserManagementRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator;

    public const string StudentManagementRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator + "," +
        CoordinatorAlias + "," + Coordinator + "," +
        AttendantAlias + "," + Attendant;

    public const string StudentRecordRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator + "," +
        CoordinatorAlias + "," + Coordinator + "," +
        ProfessionalAlias + "," + Professional + "," +
        AttendantAlias + "," + Attendant;

    public const string ProfessionalManagementRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator + "," +
        CoordinatorAlias + "," + Coordinator;

    public const string CourseManagementRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator + "," +
        TeacherAlias + "," + Teacher;

    public const string FinancialManagementRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator;

    public const string CourseAccessRoles =
        SuperAdmin + "," + AdministratorAlias + "," + Administrator + "," +
        TeacherAlias + "," + Teacher + "," +
        StudentAlias + "," + Student + "," +
        FamilyMemberAlias + "," + FamilyMember;

    public static readonly string[] UserManagement =
    [
        AdministratorAlias,
        Administrator,
        SuperAdmin
    ];

    public static readonly string[] StudentManagement =
    [
        AdministratorAlias,
        Administrator,
        SuperAdmin,
        CoordinatorAlias,
        Coordinator,
        AttendantAlias,
        Attendant
    ];

    public static readonly string[] StudentRecords =
    [
        AdministratorAlias,
        Administrator,
        SuperAdmin,
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
        SuperAdmin,
        CoordinatorAlias,
        Coordinator
    ];

    public static readonly string[] CourseManagement =
    [
        AdministratorAlias,
        Administrator,
        SuperAdmin,
        TeacherAlias,
        Teacher
    ];

    public static readonly string[] FinancialManagement =
    [
        AdministratorAlias,
        Administrator,
        SuperAdmin
    ];

    public static readonly string[] CourseAccess =
    [
        AdministratorAlias,
        Administrator,
        SuperAdmin,
        TeacherAlias,
        Teacher,
        StudentAlias,
        Student,
        FamilyMemberAlias,
        FamilyMember
    ];

    public static readonly string[] All =
    [
        SuperAdmin,
        Administrator,
        Coordinator,
        Professional,
        Attendant,
        Teacher,
        Student,
        FamilyMember
    ];

    public static readonly ApplicationRole[] Seed =
    [
        Create("00000000-0000-0000-0000-000000000001", SuperAdmin, "System owner with global access."),
        Create("11111111-1111-1111-1111-111111111111", Administrator, "Tenant administrator."),
        Create("22222222-2222-2222-2222-222222222222", Coordinator, "Clinical and operational coordinator."),
        Create("33333333-3333-3333-3333-333333333333", Professional, "Clinical professional."),
        Create("44444444-4444-4444-4444-444444444444", Attendant, "Front-desk and intake attendant."),
        Create("55555555-5555-5555-5555-555555555555", Teacher, "EAD teacher."),
        Create("66666666-6666-6666-6666-666666666666", Student, "Student portal user."),
        Create("77777777-7777-7777-7777-777777777777", FamilyMember, "Family portal user.")
    ];

    private static ApplicationRole Create(string id, string name, string description)
    {
        return new ApplicationRole
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            ConcurrencyStamp = id,
            Description = description,
            IsSystemRole = true,
            CreatedAt = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
