using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PastaneApp.Core.Entities;

namespace PastaneApp.Data.Seed;

public static class IdentitySeeder
{
    private const string AdminRole = "Admin";
    private const string CustomerRole = "Customer";
    private const string DefaultAdminEmail = "admin@pastane.com";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        foreach (var role in new[] { AdminRole, CustomerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = configuration["Seed:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = DefaultAdminEmail;
        }

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }

        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Yönetici hesabı oluşturulmadı: Seed:AdminPassword ayarı tanımlı değil. " +
                "İlk kurulumda ortam değişkeni olarak verin (Seed__AdminPassword).");
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Yönetici",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            logger.LogError(
                "Yönetici hesabı oluşturulamadı: {Errors}",
                string.Join(" ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(adminUser, AdminRole);
    }
}
