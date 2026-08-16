namespace PastaneApp.Core.Entities;

public class Allergen : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconClass { get; set; }

    public ICollection<ProductAllergen> ProductAllergens { get; set; } = new List<ProductAllergen>();
}
