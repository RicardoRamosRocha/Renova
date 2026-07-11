using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Identity;
using Renova.Web.Areas.Admin.ViewModels.Users;
using Renova.Web.Services;
using Renova.Web.ViewModels;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class UsersController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IPhotoService photoService,
    ICurrentTenantService currentTenantService) : AdminControllerBase
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(string? search, bool? active, int page = 1)
    {
        var query = userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(user =>
                user.FullName.ToLower().Contains(term) ||
                (user.Email != null && user.Email.ToLower().Contains(term)));
        }

        if (active.HasValue)
        {
            query = query.Where(user => user.IsActive == active.Value);
        }

        ViewBag.Search = search;
        ViewBag.Active = active;
        ViewBag.Total = await userManager.Users.CountAsync();
        ViewBag.ActiveCount = await userManager.Users.CountAsync(user => user.IsActive);
        ViewBag.InactiveCount = await userManager.Users.CountAsync(user => !user.IsActive);
        ViewBag.RolesCount = await roleManager.Roles.CountAsync();

        const int pageSize = 10;
        page = Math.Max(1, page);
        var totalItems = await query.CountAsync();
        var users = await query
            .OrderBy(user => user.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new PagedResult<ApplicationUser>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        ViewBag.Roles = await userManager.GetRolesAsync(user);
        return View(user);
    }

    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync();
        return View(new UserFormViewModel { TemporaryPassword = "Renova@123" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return View(model);
        }

        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            await LoadRolesAsync();
            return View(model);
        }

        string? photoPath;

        try
        {
            photoPath = model.RemovePhoto ? null : await photoService.SavePhotoAsync(model.Photo, "users");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            await LoadRolesAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            PhoneNumber = model.Phone?.Trim(),
            IsActive = model.IsActive,
            PhotoPath = photoPath,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, string.IsNullOrWhiteSpace(model.TemporaryPassword) ? "Renova@123" : model.TemporaryPassword);
        if (!result.Succeeded)
        {
            photoService.DeletePhoto(photoPath);
            AddIdentityErrors(result);
            await LoadRolesAsync();
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.Role))
        {
            await userManager.AddToRoleAsync(user, model.Role);
        }

        var tenantClaimResult = await EnsureTenantClaimAsync(user);
        if (!tenantClaimResult.Succeeded)
        {
            AddIdentityErrors(tenantClaimResult);
            await LoadRolesAsync();
            return View(model);
        }

        TempData["Success"] = "Usuário cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        await LoadRolesAsync();
        return View(new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            IsActive = user.IsActive,
            Role = roles.FirstOrDefault(),
            PhotoPath = user.PhotoPath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid)
        {
            await LoadRolesAsync();
            return View(model);
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.PhoneNumber = model.Phone?.Trim();
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            if (model.RemovePhoto)
            {
                photoService.DeletePhoto(user.PhotoPath);
                user.PhotoPath = null;
            }
            else
            {
                user.PhotoPath = await photoService.SavePhotoAsync(model.Photo, "users", user.PhotoPath);
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            model.PhotoPath = user.PhotoPath;
            await LoadRolesAsync();
            return View(model);
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            await LoadRolesAsync();
            return View(model);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        if (!string.IsNullOrWhiteSpace(model.Role))
        {
            await userManager.AddToRoleAsync(user, model.Role);
        }

        if (!string.IsNullOrWhiteSpace(model.TemporaryPassword))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, model.TemporaryPassword);
            if (!resetResult.Succeeded)
            {
                AddIdentityErrors(resetResult);
                await LoadRolesAsync();
                return View(model);
            }
        }

        var tenantClaimResult = await EnsureTenantClaimAsync(user);
        if (!tenantClaimResult.Succeeded)
        {
            AddIdentityErrors(tenantClaimResult);
            await LoadRolesAsync();
            return View(model);
        }

        TempData["Success"] = "Usuário atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inactivate(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        TempData["Success"] = "Usuário inativado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            photoService.DeletePhoto(user.PhotoPath);
            TempData["Success"] = "Usuário excluído com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        TempData["Success"] = "Usuário não pôde ser excluído e foi inativado com segurança.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesAsync()
    {
        ViewBag.Roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();
    }

    private async Task<IdentityResult> EnsureTenantClaimAsync(ApplicationUser user)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "TenantNotResolved",
                Description = MissingTenantMessage
            });
        }

        var claims = await userManager.GetClaimsAsync(user);
        var tenantClaims = claims
            .Where(claim => claim.Type == CurrentTenantService.TenantIdClaimType)
            .ToList();

        if (tenantClaims.Count == 1 && tenantClaims[0].Value == tenantId.Value.ToString())
        {
            return IdentityResult.Success;
        }

        foreach (var tenantClaim in tenantClaims)
        {
            var removeResult = await userManager.RemoveClaimAsync(user, tenantClaim);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }
        }

        return await userManager.AddClaimAsync(
            user,
            new Claim(CurrentTenantService.TenantIdClaimType, tenantId.Value.ToString()));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
