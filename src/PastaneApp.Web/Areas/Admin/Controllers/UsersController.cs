using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Areas.Admin.Models;

namespace PastaneApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private const string AdminRole = "Admin";
    private const string CustomerRole = "Customer";
    private static readonly string[] AllowedRoles = { AdminRole, CustomerRole };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var orders = await _unitOfWork.Repository<Order>().GetAllAsync();
        var orderCounts = orders.GroupBy(o => o.ApplicationUserId).ToDictionary(g => g.Key, g => g.Count());
        var currentUserId = _userManager.GetUserId(User);

        var items = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "-",
                OrderCount = orderCounts.GetValueOrDefault(user.Id),
                IsLocked = IsLocked(user),
                IsCurrentUser = user.Id == currentUserId
            });
        }

        return View(items);
    }

    public IActionResult Create()
    {
        return View(new UserFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Şifre zorunludur.");
        }

        if (!AllowedRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Geçersiz rol.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName?.Trim(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            Address = model.Address?.Trim()
        };

        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["Success"] = "Kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return View(new UserFormViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Role = roles.FirstOrDefault() ?? CustomerRole,
            IsLocked = IsLocked(user),
            IsCurrentUser = user.Id == _userManager.GetUserId(User)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var isCurrentUser = user.Id == _userManager.GetUserId(User);
        model.IsCurrentUser = isCurrentUser;
        model.Email = user.Email ?? string.Empty;
        ModelState.Remove(nameof(model.Email));

        if (!AllowedRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Geçersiz rol.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault() ?? CustomerRole;

        if (isCurrentUser && model.Role != currentRole)
        {
            ModelState.AddModelError(nameof(model.Role), "Kendi rolünüzü değiştiremezsiniz.");
        }

        if (isCurrentUser && model.IsLocked)
        {
            ModelState.AddModelError(nameof(model.IsLocked), "Kendi hesabınızı kilitleyemezsiniz.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.FullName = model.FullName?.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        user.Address = model.Address?.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddErrors(updateResult);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!passwordResult.Succeeded)
            {
                AddErrors(passwordResult);
                return View(model);
            }
        }

        if (model.Role != currentRole)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        if (model.IsLocked != IsLocked(user))
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, model.IsLocked ? DateTimeOffset.MaxValue : null);
            if (!model.IsLocked)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }
        }

        TempData["Success"] = "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var model = await BuildDeleteModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var model = await BuildDeleteModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        if (model.IsCurrentUser)
        {
            TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
            return RedirectToAction(nameof(Index));
        }

        if (model.OrderCount > 0)
        {
            TempData["Error"] = "Siparişi olan kullanıcı silinemez, bunun yerine hesabı kilitleyebilirsiniz.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        var result = await _userManager.DeleteAsync(user!);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Kullanıcı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<UserDeleteViewModel?> BuildDeleteModelAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        var orders = await _unitOfWork.Repository<Order>().FindAsync(o => o.ApplicationUserId == id);
        return new UserDeleteViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            OrderCount = orders.Count,
            IsCurrentUser = user.Id == _userManager.GetUserId(User)
        };
    }

    private static bool IsLocked(ApplicationUser user) =>
        user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
