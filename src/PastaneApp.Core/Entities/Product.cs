namespace PastaneApp.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ServingInfo { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductIngredient> Ingredients { get; set; } = new List<ProductIngredient>();
    public ICollection<ProductAllergen> ProductAllergens { get; set; } = new List<ProductAllergen>();
}
