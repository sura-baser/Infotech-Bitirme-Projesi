namespace PastaneApp.Web.Models.Products;

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
