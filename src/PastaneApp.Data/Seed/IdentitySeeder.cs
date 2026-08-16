using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PastaneApp.Core.Entities;

namespace PastaneApp.Data.Seed;

public static class IdentitySeeder
{
    private const string AdminRole = "Admin";
    private const string CustomerRole = "Customer";
    private const string AdminEmail = "admin@pastane.com";
    private const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { AdminRole, CustomerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "Yönetici",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }
}
