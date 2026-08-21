using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Models.Cart;

namespace PastaneApp.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await GetOrCreateCartAsync();

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToHashSet();
        var products = (await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Images))
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id);

        var model = new CartIndexViewModel
        {
            Items = cart.CartItems.Select(ci => new CartItemViewModel
            {
                CartItemId = ci.Id,
                ProductId = ci.ProductId,
                ProductName = products.TryGetValue(ci.ProductId, out var p) ? p.Name : "Ürün bulunamadı",
                UnitPrice = products.TryGetValue(ci.ProductId, out var p2) ? p2.Price : 0,
                Quantity = ci.Quantity,
                ImageUrl = products.TryGetValue(ci.ProductId, out var p3)
                    ? p3.Images.Where(i => i.ImageType == ImageType.Finished).OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
                    : null
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        var cart = await GetOrCreateCartAsync();

        var existingItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
        var existing = existingItems.FirstOrDefault();

        if (existing is not null)
        {
            existing.Quantity += quantity;
            _unitOfWork.Repository<CartItem>().Update(existing);
        }
        else
        {
            await _unitOfWork.Repository<CartItem>().AddAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            });
        }

        await _unitOfWork.CompleteAsync();
        TempData["Success"] = "Ürün sepete eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var item = await ValidateOwnedCartItemAsync(cartItemId);
        if (item is null)
        {
            return NotFound();
        }

        if (quantity <= 0)
        {
            _unitOfWork.Repository<CartItem>().Remove(item);
        }
        else
        {
            item.Quantity = quantity;
            _unitOfWork.Repository<CartItem>().Update(item);
        }

        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var item = await ValidateOwnedCartItemAsync(cartItemId);
        if (item is null)
        {
            return NotFound();
        }

        _unitOfWork.Repository<CartItem>().Remove(item);
        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<CartItem?> ValidateOwnedCartItemAsync(int cartItemId)
    {
        var item = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
        if (item is null)
        {
            return null;
        }

        var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(item.CartId);
        if (cart is null || cart.ApplicationUserId != GetUserId())
        {
            return null;
        }

        return item;
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
        var userId = GetUserId();
        var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.ApplicationUserId == userId);
        var cart = carts.FirstOrDefault();

        if (cart is null)
        {
            cart = new Cart { ApplicationUserId = userId };
            await _unitOfWork.Repository<Cart>().AddAsync(cart);
            await _unitOfWork.CompleteAsync();
        }
        else
        {
            cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cart.Id, c => c.CartItems) ?? cart;
        }

        return cart;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
