using Microsoft.EntityFrameworkCore;
using PastaneApp.Core.Entities;

namespace PastaneApp.Data.Seed;

public static class AllergenSeeder
{
    private static readonly string[] DefaultAllergens =
    {
        "Gluten", "Süt/Laktoz", "Yumurta", "Fındık/Kuruyemiş", "Soya", "Susam"
    };

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Allergens.AnyAsync())
        {
            return;
        }

        foreach (var name in DefaultAllergens)
        {
            context.Allergens.Add(new Allergen { Name = name });
        }

        await context.SaveChangesAsync();
    }
}
