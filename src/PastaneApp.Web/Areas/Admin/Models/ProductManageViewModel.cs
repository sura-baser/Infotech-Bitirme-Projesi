using PastaneApp.Core.Entities;

namespace PastaneApp.Web.Areas.Admin.Models;

public class ProductManageViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductIngredient> Ingredients { get; set; } = new();
    public List<Allergen> AllAllergens { get; set; } = new();
    public HashSet<int> SelectedAllergenIds { get; set; } = new();
}
