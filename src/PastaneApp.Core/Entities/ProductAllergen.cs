namespace PastaneApp.Core.Entities;

public class ProductAllergen : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int AllergenId { get; set; }
    public Allergen Allergen { get; set; } = null!;
}
