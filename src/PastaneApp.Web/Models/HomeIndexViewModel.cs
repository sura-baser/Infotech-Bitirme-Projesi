using PastaneApp.Core.Entities;
using PastaneApp.Web.Models.Products;

namespace PastaneApp.Web.Models;

public class HomeIndexViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<ProductCardViewModel> FeaturedProducts { get; set; } = new();
}
