namespace PastaneApp.Web.Models.Cart;

public class CartIndexViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
}
