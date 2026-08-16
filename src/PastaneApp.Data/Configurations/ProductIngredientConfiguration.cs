using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastaneApp.Core.Entities;

namespace PastaneApp.Data.Configurations;

public class ProductIngredientConfiguration : IEntityTypeConfiguration<ProductIngredient>
{
    public void Configure(EntityTypeBuilder<ProductIngredient> builder)
    {
        builder.Property(i => i.Name).IsRequired().HasMaxLength(150);
    }
}
