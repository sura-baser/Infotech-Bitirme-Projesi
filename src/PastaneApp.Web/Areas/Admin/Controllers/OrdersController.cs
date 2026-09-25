using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Services;

namespace PastaneApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _orderService.GetAllOrdersAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderWithCustomerAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        if (!await _orderService.UpdateStatusAsync(id, status))
        {
            return NotFound();
        }

        TempData["Success"] = "Sipariş durumu güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
