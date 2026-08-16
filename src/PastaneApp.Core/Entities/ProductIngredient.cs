namespace PastaneApp.Core.Entities;

public class ProductIngredient : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
