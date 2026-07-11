using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Application.Security;
using Renova.Infrastructure.Data;
using Renova.Infrastructure.Data.Seed;
using Renova.Infrastructure.Identity;
using Renova.Infrastructure.Security;
using Renova.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

builder.Services.AddScoped<IPermissionService, PermissionService>();

var app = builder.Build();

await app.MigrateDatabaseAsync();
await app.SeedTenantBootstrapAsync();
await app.SeedIdentityAsync();
await app.SeedTenantUserClaimsAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/web-login", async (
    HttpContext httpContext,
    UserManager<ApplicationUser> userManager,
    IDbContextFactory<AppDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string? returnUrl) =>
{
    var user = await userManager.FindByEmailAsync(email.Trim());

    if (user is null ||
        !user.IsActive ||
        !await userManager.IsEmailConfirmedAsync(user) ||
        !await userManager.CheckPasswordAsync(user, password))
    {
        return Results.Redirect(GetLoginErrorUrl(returnUrl));
    }

    var roles = await userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.FullName),
        new(ClaimTypes.Email, user.Email ?? string.Empty)
    };

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var userClaims = await userManager.GetClaimsAsync(user);
    claims.AddRange(userClaims.Where(claim => claim.Type != CurrentTenantService.TenantIdClaimType));

    if (!claims.Any(claim => claim.Type == CurrentTenantService.TenantIdClaimType))
    {
        var tenantId = await ResolveLoginTenantIdAsync(dbContextFactory, environment, userClaims);
        if (tenantId.HasValue)
        {
            claims.Add(new Claim(CurrentTenantService.TenantIdClaimType, tenantId.Value.ToString()));
        }
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

    return Results.Redirect(GetSafeReturnUrl(returnUrl));
}).DisableAntiforgery();

app.MapPost("/auth/web-logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).DisableAntiforgery();

app.MapGet("/", () => Results.Redirect("/Admin"));

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "crm",
    areaName: "CRM",
    pattern: "CRM/{controller=Students}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "medical",
    areaName: "Medical",
    pattern: "Medical/{controller=MedicalRecords}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "ead",
    areaName: "EAD",
    pattern: "EAD/{controller=Courses}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "finance",
    areaName: "Finance",
    pattern: "Finance/{controller=Finance}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "reports",
    areaName: "Reports",
    pattern: "Reports/{controller=Reports}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "settings",
    areaName: "Settings",
    pattern: "Settings/{controller=Settings}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string GetSafeReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        return "/Admin";
    }

    return returnUrl[0] == '/'
        && (returnUrl.Length == 1 || returnUrl[1] is not '/' and not '\\')
        && !returnUrl.Contains("://", StringComparison.Ordinal)
            ? returnUrl
            : "/Admin";
}

static string GetLoginErrorUrl(string? returnUrl)
{
    var safeReturnUrl = GetSafeReturnUrl(returnUrl);
    return $"/login?error=1&returnUrl={Uri.EscapeDataString(safeReturnUrl)}";
}

static async Task<Guid?> ResolveLoginTenantIdAsync(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    IEnumerable<Claim> userClaims)
{
    await using var db = await dbContextFactory.CreateDbContextAsync();

    foreach (var claim in userClaims.Where(item => item.Type == CurrentTenantService.TenantIdClaimType))
    {
        if (!Guid.TryParse(claim.Value, out var claimTenantId) || claimTenantId == Guid.Empty)
        {
            continue;
        }

        if (await db.Tenants.AnyAsync(tenant => tenant.Id == claimTenantId && tenant.IsActive))
        {
            return claimTenantId;
        }
    }

    if (!environment.IsDevelopment())
    {
        return null;
    }

    var activeTenants = await db.Tenants
        .AsNoTracking()
        .Where(tenant => tenant.IsActive)
        .OrderBy(tenant => tenant.CreatedAt)
        .Select(tenant => tenant.Id)
        .Take(2)
        .ToListAsync();

    return activeTenants.Count == 1 ? activeTenants[0] : null;
}
