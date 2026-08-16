using PastaneApp.Core.Enums;

namespace PastaneApp.Core.Entities;

public class ProductImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public ImageType ImageType { get; set; }
    public int SortOrder { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
