namespace PastaneApp.Core.Services;

public record CartLine(
    int CartItemId,
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? CustomizationNotes,
    string? ImageUrl)
{
    public decimal Subtotal => UnitPrice * Quantity;
}

public record CartSummary(IReadOnlyList<CartLine> Lines)
{
    public decimal Total => Lines.Sum(l => l.Subtotal);
    public bool IsEmpty => Lines.Count == 0;
}

public interface ICartService
{
    Task<CartSummary> GetCartAsync(string userId);
    Task<ServiceResult> AddProductAsync(string userId, int productId, int quantity);
    Task<ServiceResult> AddPartyBoxAsync(string userId, int boxProductId, IReadOnlyDictionary<string, int> quantities);
    Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity);
    Task<bool> RemoveItemAsync(string userId, int cartItemId);
}
