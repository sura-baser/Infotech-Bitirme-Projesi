using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Core.Services;

namespace PastaneApp.Services;

public class CartService : ICartService
{
    private const int MaxQuantityPerProduct = 2;

    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartSummary> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToHashSet();
        var products = (await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Images))
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id);

        var lines = cart.CartItems.Select(ci =>
        {
            products.TryGetValue(ci.ProductId, out var product);
            return new CartLine(
                ci.Id,
                ci.ProductId,
                product?.Name ?? "Ürün bulunamadı",
                product?.Price ?? 0,
                ci.Quantity,
                ci.CustomizationNotes,
                product?.Images
                    .Where(i => i.ImageType == ImageType.Finished)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault());
        }).ToList();

        return new CartSummary(lines);
    }

    public async Task<ServiceResult> AddProductAsync(string userId, int productId, int quantity)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        if (product is null || !product.IsActive)
        {
            return ServiceResult.NotFound();
        }

        quantity = Math.Max(quantity, 1);
        var cart = await GetOrCreateCartAsync(userId);

        var existing = (await _unitOfWork.Repository<CartItem>()
            .FindAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId)).FirstOrDefault();

        if (existing is not null)
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, MaxQuantityPerProduct);
            _unitOfWork.Repository<CartItem>().Update(existing);
        }
        else
        {
            await _unitOfWork.Repository<CartItem>().AddAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = Math.Min(quantity, MaxQuantityPerProduct)
            });
        }

        await _unitOfWork.CompleteAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AddPartyBoxAsync(string userId, int boxProductId, IReadOnlyDictionary<string, int> quantities)
    {
        var boxProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(boxProductId);
        if (boxProduct is null || !boxProduct.IsActive)
        {
            return ServiceResult.NotFound();
        }

        var pieceCount = PartyBoxRules.ParsePieceCount(boxProduct.ServingInfo);
        var selected = quantities.Where(kv => kv.Value > 0).ToList();
        var totalSelected = selected.Sum(kv => kv.Value);

        if (pieceCount <= 0 || totalSelected != pieceCount)
        {
            return ServiceResult.Invalid($"Seçtiğin toplam parça sayısı ({totalSelected}) kutunun boyutuyla ({pieceCount} parça) eşleşmiyor.");
        }

        var notes = string.Join(", ", selected.Select(kv => $"{kv.Value}x {kv.Key}"));

        var cart = await GetOrCreateCartAsync(userId);
        await _unitOfWork.Repository<CartItem>().AddAsync(new CartItem
        {
            CartId = cart.Id,
            ProductId = boxProduct.Id,
            Quantity = 1,
            CustomizationNotes = notes
        });

        await _unitOfWork.CompleteAsync();
        return ServiceResult.Ok();
    }

    public async Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity)
    {
        var item = await GetOwnedCartItemAsync(userId, cartItemId);
        if (item is null)
        {
            return false;
        }

        if (quantity <= 0)
        {
            _unitOfWork.Repository<CartItem>().Remove(item);
        }
        else
        {
            item.Quantity = Math.Min(quantity, MaxQuantityPerProduct);
            _unitOfWork.Repository<CartItem>().Update(item);
        }

        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(string userId, int cartItemId)
    {
        var item = await GetOwnedCartItemAsync(userId, cartItemId);
        if (item is null)
        {
            return false;
        }

        _unitOfWork.Repository<CartItem>().Remove(item);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    private async Task<CartItem?> GetOwnedCartItemAsync(string userId, int cartItemId)
    {
        var item = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
        if (item is null)
        {
            return null;
        }

        var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(item.CartId);
        return cart is not null && cart.ApplicationUserId == userId ? item : null;
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = (await _unitOfWork.Repository<Cart>().FindAsync(c => c.ApplicationUserId == userId)).FirstOrDefault();

        if (cart is null)
        {
            cart = new Cart { ApplicationUserId = userId };
            await _unitOfWork.Repository<Cart>().AddAsync(cart);
            await _unitOfWork.CompleteAsync();
            return cart;
        }

        return await _unitOfWork.Repository<Cart>().GetByIdAsync(cart.Id, c => c.CartItems) ?? cart;
    }
}
