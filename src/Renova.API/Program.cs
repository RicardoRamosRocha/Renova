using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Renova.API.Auth;
using Renova.Infrastructure.Data;
using Renova.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AccessTokenService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

builder.Services
    .AddAuthentication(RenovaAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, RenovaBearerAuthenticationHandler>(
        RenovaAuthenticationDefaults.AuthenticationScheme,
        options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserManagement", policy =>
        policy.RequireRole(ApplicationRoles.UserManagement));

    options.AddPolicy("FinancialManagement", policy =>
        policy.RequireRole(ApplicationRoles.FinancialManagement));

    options.AddPolicy("ClinicalAccess", policy =>
        policy.RequireRole(ApplicationRoles.StudentRecords));

    options.AddPolicy("CourseManagement", policy =>
        policy.RequireRole(ApplicationRoles.CourseManagement));

    options.AddPolicy("CourseAccess", policy =>
        policy.RequireRole(ApplicationRoles.CourseAccess));

    options.AddPolicy("StudentLearning", policy =>
        policy.RequireRole(ApplicationRoles.CourseAccess));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
