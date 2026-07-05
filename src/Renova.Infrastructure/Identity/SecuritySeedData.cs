using Renova.Domain.Security;

namespace Renova.Infrastructure.Identity;

internal static class SecuritySeedData
{
    public static readonly IReadOnlyList<ModuleSeed> Modules =
    [
        new(new Guid("10000000-0000-0000-0000-000000000001"), "Admin", "Admin", "Global administration.", "bi-shield-lock", 1),
        new(new Guid("10000000-0000-0000-0000-000000000002"), "CRM", "CRM", "Students, families and relationship management.", "bi-people", 2),
        new(new Guid("10000000-0000-0000-0000-000000000003"), "Prontuario", "Prontuario", "Medical records and clinical evolution.", "bi-clipboard2-pulse", 3),
        new(new Guid("10000000-0000-0000-0000-000000000004"), "EAD", "EAD", "Courses and learning tracks.", "bi-mortarboard", 4),
        new(new Guid("10000000-0000-0000-0000-000000000005"), "Financeiro", "Financeiro", "Payments and financial management.", "bi-cash-coin", 5),
        new(new Guid("10000000-0000-0000-0000-000000000006"), "Estoque", "Estoque", "Inventory management.", "bi-box-seam", 6),
        new(new Guid("10000000-0000-0000-0000-000000000007"), "PlanosTerapêuticos", "PlanosTerapeuticos", "Therapeutic plans.", "bi-heart-pulse", 7),
        new(new Guid("10000000-0000-0000-0000-000000000008"), "Relatórios", "Relatorios", "Reports and dashboards.", "bi-graph-up", 8),
        new(new Guid("10000000-0000-0000-0000-000000000009"), "PortalDaFamilia", "PortalDaFamilia", "Family portal.", "bi-house-heart", 9),
        new(new Guid("10000000-0000-0000-0000-000000000010"), "Configuracoes", "Configuracoes", "System settings.", "bi-gear", 10)
    ];

    public static readonly IReadOnlyList<PermissionSeed> Permissions =
    [
        Permission("20000000-0000-0000-0000-000000000001", "Admin access", ApplicationPermissions.AdminAccess, "Access administrative area.", Modules[0].Id),
        Permission("20000000-0000-0000-0000-000000000002", "Manage tenants", ApplicationPermissions.TenantsManage, "Manage SaaS tenants.", Modules[0].Id),
        Permission("20000000-0000-0000-0000-000000000003", "Manage users", ApplicationPermissions.UsersManage, "Manage users and roles.", Modules[0].Id),
        Permission("20000000-0000-0000-0000-000000000004", "View students", ApplicationPermissions.StudentsView, "View students.", Modules[1].Id),
        Permission("20000000-0000-0000-0000-000000000005", "Create students", ApplicationPermissions.StudentsCreate, "Create students.", Modules[1].Id),
        Permission("20000000-0000-0000-0000-000000000006", "Edit students", ApplicationPermissions.StudentsEdit, "Edit students.", Modules[1].Id),
        Permission("20000000-0000-0000-0000-000000000007", "Delete students", ApplicationPermissions.StudentsDelete, "Delete students.", Modules[1].Id),
        Permission("20000000-0000-0000-0000-000000000008", "View medical records", ApplicationPermissions.MedicalRecordsView, "View medical records.", Modules[2].Id),
        Permission("20000000-0000-0000-0000-000000000009", "Manage medical records", ApplicationPermissions.MedicalRecordsManage, "Manage medical records.", Modules[2].Id),
        Permission("20000000-0000-0000-0000-000000000010", "Access EAD", ApplicationPermissions.EadAccess, "Access EAD module.", Modules[3].Id),
        Permission("20000000-0000-0000-0000-000000000011", "Access finance", ApplicationPermissions.FinanceAccess, "Access finance module.", Modules[4].Id),
        Permission("20000000-0000-0000-0000-000000000012", "Access inventory", ApplicationPermissions.InventoryAccess, "Access inventory module.", Modules[5].Id),
        Permission("20000000-0000-0000-0000-000000000013", "Access therapeutic plans", ApplicationPermissions.TherapeuticPlansAccess, "Access therapeutic plans.", Modules[6].Id),
        Permission("20000000-0000-0000-0000-000000000014", "Access reports", ApplicationPermissions.ReportsAccess, "Access reports.", Modules[7].Id),
        Permission("20000000-0000-0000-0000-000000000015", "Access family portal", ApplicationPermissions.FamilyPortalAccess, "Access family portal.", Modules[8].Id),
        Permission("20000000-0000-0000-0000-000000000016", "Access settings", ApplicationPermissions.SettingsAccess, "Access settings.", Modules[9].Id)
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> RolePermissions =
        new Dictionary<string, IReadOnlyCollection<string>>
        {
            [ApplicationRoles.SuperAdmin] = ApplicationPermissions.All.ToArray(),
            [ApplicationRoles.Administrator] = ApplicationPermissions.All
                .Where(permission => permission != ApplicationPermissions.TenantsManage)
                .ToArray(),
            [ApplicationRoles.Coordinator] =
            [
                ApplicationPermissions.StudentsView,
                ApplicationPermissions.StudentsCreate,
                ApplicationPermissions.StudentsEdit,
                ApplicationPermissions.MedicalRecordsView,
                ApplicationPermissions.MedicalRecordsManage,
                ApplicationPermissions.TherapeuticPlansAccess,
                ApplicationPermissions.ReportsAccess
            ],
            [ApplicationRoles.Professional] =
            [
                ApplicationPermissions.StudentsView,
                ApplicationPermissions.MedicalRecordsView,
                ApplicationPermissions.MedicalRecordsManage,
                ApplicationPermissions.TherapeuticPlansAccess
            ],
            [ApplicationRoles.Attendant] =
            [
                ApplicationPermissions.StudentsView,
                ApplicationPermissions.StudentsCreate,
                ApplicationPermissions.StudentsEdit
            ],
            [ApplicationRoles.Teacher] =
            [
                ApplicationPermissions.EadAccess,
                ApplicationPermissions.StudentsView
            ],
            [ApplicationRoles.FamilyMember] =
            [
                ApplicationPermissions.FamilyPortalAccess
            ],
            [ApplicationRoles.Student] =
            [
                ApplicationPermissions.EadAccess
            ]
        };

    private static PermissionSeed Permission(string id, string name, string key, string description, Guid moduleId)
    {
        return new PermissionSeed(new Guid(id), name, key, description, moduleId);
    }

    public sealed record ModuleSeed(Guid Id, string Name, string Key, string Description, string Icon, int DisplayOrder);

    public sealed record PermissionSeed(Guid Id, string Name, string Key, string Description, Guid ModuleId);
}
