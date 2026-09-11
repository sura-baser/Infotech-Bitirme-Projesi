using PastaneApp.Web.Models.Products;

namespace PastaneApp.Web.Models;

public class HomeIndexViewModel
{
    public List<CategoryCardViewModel> Categories { get; set; } = new();
    public List<ProductCardViewModel> FeaturedProducts { get; set; } = new();
}

public class CategoryCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
