using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;

namespace PastaneApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _unitOfWork.Repository<Order>().GetAllAsync(o => o.ApplicationUser);
        return View(orders.OrderByDescending(o => o.OrderDate).ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id, o => o.ApplicationUser, o => o.OrderDetails);
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
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = status;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Sipariş durumu güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
