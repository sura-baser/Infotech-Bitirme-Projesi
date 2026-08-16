using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastaneApp.Core.Entities;

namespace PastaneApp.Data.Configurations;

public class AllergenConfiguration : IEntityTypeConfiguration<Allergen>
{
    public void Configure(EntityTypeBuilder<Allergen> builder)
    {
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.IconClass).HasMaxLength(100);
        builder.HasIndex(a => a.Name).IsUnique();
    }
}
