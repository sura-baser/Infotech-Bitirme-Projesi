using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastaneApp.Core.Entities;

namespace PastaneApp.Data.Configurations;

public class ProductAllergenConfiguration : IEntityTypeConfiguration<ProductAllergen>
{
    public void Configure(EntityTypeBuilder<ProductAllergen> builder)
    {
        builder.HasIndex(pa => new { pa.ProductId, pa.AllergenId }).IsUnique();

        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAllergens)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Allergen)
            .WithMany(a => a.ProductAllergens)
            .HasForeignKey(pa => pa.AllergenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
