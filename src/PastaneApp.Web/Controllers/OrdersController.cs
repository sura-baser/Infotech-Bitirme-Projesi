using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Models.Orders;

namespace PastaneApp.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _unitOfWork.Repository<Order>().FindAsync(o => o.ApplicationUserId == GetUserId());
        return View(orders.OrderByDescending(o => o.OrderDate).ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id, o => o.OrderDetails);
        if (order is null || order.ApplicationUserId != GetUserId())
        {
            return NotFound();
        }

        return View(order);
    }

    public async Task<IActionResult> Checkout()
    {
        var cart = await GetCartAsync();
        if (cart is null || !cart.CartItems.Any())
        {
            TempData["Error"] = "Sepetiniz boş.";
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userManager.GetUserAsync(User);
        var model = new CheckoutViewModel
        {
            DeliveryAddress = user?.Address ?? string.Empty,
            PhoneNumber = user?.PhoneNumber ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = await GetCartAsync();
        if (cart is null || !cart.CartItems.Any())
        {
            TempData["Error"] = "Sepetiniz boş.";
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToHashSet();
        var products = (await _unitOfWork.Repository<Product>().GetAllAsync())
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id);

        var order = new Order
        {
            ApplicationUserId = GetUserId(),
            DeliveryAddress = model.DeliveryAddress,
            PhoneNumber = model.PhoneNumber,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        decimal total = 0;
        foreach (var item in cart.CartItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                continue;
            }

            total += product.Price * item.Quantity;
            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            });

            product.Stock = Math.Max(0, product.Stock - item.Quantity);
            _unitOfWork.Repository<Product>().Update(product);
        }

        order.TotalAmount = total;

        await _unitOfWork.Repository<Order>().AddAsync(order);

        foreach (var item in cart.CartItems.ToList())
        {
            _unitOfWork.Repository<CartItem>().Remove(item);
        }

        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Siparişiniz oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    private async Task<Cart?> GetCartAsync()
    {
        var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.ApplicationUserId == GetUserId());
        var cart = carts.FirstOrDefault();
        if (cart is null)
        {
            return null;
        }

        return await _unitOfWork.Repository<Cart>().GetByIdAsync(cart.Id, c => c.CartItems);
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
