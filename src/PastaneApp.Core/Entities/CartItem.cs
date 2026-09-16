namespace PastaneApp.Core.Entities;

public class CartItem : BaseEntity
{
    public int Quantity { get; set; }
    public string? CustomizationNotes { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
