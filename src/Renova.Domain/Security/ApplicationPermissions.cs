namespace Renova.Domain.Security;

public static class ApplicationPermissions
{
    public const string AdminAccess = "Admin.Access";
    public const string TenantsManage = "Tenants.Manage";
    public const string UsersManage = "Users.Manage";
    public const string StudentsView = "Students.View";
    public const string StudentsCreate = "Students.Create";
    public const string StudentsEdit = "Students.Edit";
    public const string StudentsDelete = "Students.Delete";
    public const string MedicalRecordsView = "MedicalRecords.View";
    public const string MedicalRecordsManage = "MedicalRecords.Manage";
    public const string EadAccess = "Ead.Access";
    public const string FinanceAccess = "Finance.Access";
    public const string InventoryAccess = "Inventory.Access";
    public const string TherapeuticPlansAccess = "TherapeuticPlans.Access";
    public const string ReportsAccess = "Reports.Access";
    public const string FamilyPortalAccess = "FamilyPortal.Access";
    public const string SettingsAccess = "Settings.Access";

    public static readonly IReadOnlyCollection<string> All =
    [
        AdminAccess,
        TenantsManage,
        UsersManage,
        StudentsView,
        StudentsCreate,
        StudentsEdit,
        StudentsDelete,
        MedicalRecordsView,
        MedicalRecordsManage,
        EadAccess,
        FinanceAccess,
        InventoryAccess,
        TherapeuticPlansAccess,
        ReportsAccess,
        FamilyPortalAccess,
        SettingsAccess
    ];
}
