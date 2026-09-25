using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Services;
using PastaneApp.Web.Models.Cart;

namespace PastaneApp.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());

        var model = new CartIndexViewModel
        {
            Items = cart.Lines.Select(l => new CartItemViewModel
            {
                CartItemId = l.CartItemId,
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
                CustomizationNotes = l.CustomizationNotes,
                ImageUrl = l.ImageUrl
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity)
    {
        var result = await _cartService.AddProductAsync(GetUserId(), productId, quantity);
        if (!result.Success)
        {
            return NotFound();
        }

        TempData["Success"] = "Ürün sepete eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPartyBox(int boxProductId, Dictionary<string, int> quantities)
    {
        var result = await _cartService.AddPartyBoxAsync(GetUserId(), boxProductId, quantities);
        if (result.Kind == ServiceErrorKind.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Index", "PartyBox");
        }

        TempData["Success"] = "Parti kutun sepete eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        if (!await _cartService.UpdateQuantityAsync(GetUserId(), cartItemId, quantity))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        if (!await _cartService.RemoveItemAsync(GetUserId(), cartItemId))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
