using PastaneApp.Core.Entities;

namespace PastaneApp.Web.Models.Products;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<string> AllergenNames { get; set; } = new();
}
