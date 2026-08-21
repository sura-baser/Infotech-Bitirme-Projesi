using Microsoft.AspNetCore.Mvc.Rendering;
using PastaneApp.Core.Entities;

namespace PastaneApp.Web.Models.Products;

public class ProductListViewModel
{
    public List<ProductCardViewModel> Products { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<Allergen> AllAllergens { get; set; } = new();

    public int? SelectedCategoryId { get; set; }
    public List<int> ExcludedAllergenIds { get; set; } = new();
    public string? Search { get; set; }
}
