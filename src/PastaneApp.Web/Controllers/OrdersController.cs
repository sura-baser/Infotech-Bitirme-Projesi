using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Services;
using PastaneApp.Web.Models.Orders;

namespace PastaneApp.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IOrderService orderService, ICartService cartService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _orderService.GetUserOrdersAsync(GetUserId()));
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetUserOrderAsync(GetUserId(), id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    public async Task<IActionResult> Checkout()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        if (cart.IsEmpty)
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
        if (!ModelState.IsValid)
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            if (cart.IsEmpty)
            {
                TempData["Error"] = "Sepetiniz boş.";
                return RedirectToAction("Index", "Cart");
            }

            return View(model);
        }

        var result = await _orderService.CreateOrderFromCartAsync(
            GetUserId(),
            new DeliveryInfo(model.DeliveryAddress, model.DeliveryCity, model.PhoneNumber));

        if (result.CartEmpty)
        {
            TempData["Error"] = "Sepetiniz boş.";
            return RedirectToAction("Index", "Cart");
        }

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Cart");
        }

        return RedirectToAction(nameof(Pay), new { id = result.OrderId });
    }

    public async Task<IActionResult> Pay(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _orderService.StartPaymentAsync(
            GetUserId(),
            id,
            new PaymentBuyer(user?.FullName, user?.Email),
            Url.Action(nameof(PaymentCallback), "Orders", null, Request.Scheme)!,
            GetClientIp());

        switch (result.Outcome)
        {
            case PaymentStartOutcome.NotFound:
                return NotFound();

            case PaymentStartOutcome.AlreadyPaid:
                return RedirectToAction(nameof(Details), new { id });

            case PaymentStartOutcome.CompletedEarlier:
                TempData["Success"] = "Ödemeniz alındı, siparişiniz oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id });

            case PaymentStartOutcome.Unavailable:
                TempData["Error"] = result.Error;
                return RedirectToAction(nameof(Details), new { id });

            default:
                return View(new PayViewModel { Order = result.Order!, CheckoutFormHtml = result.CheckoutFormHtml! });
        }
    }

    // iyzico ödeme sonucunu kullanıcının tarayıcısı üzerinden başka bir siteden POST eder,
    // bu yüzden oturum çerezi ve antiforgery jetonu yok; siparişi ödeme jetonundan buluruz.
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PaymentCallback([FromForm] string? token)
    {
        var result = await _orderService.CompletePaymentAsync(token);

        switch (result.Outcome)
        {
            case PaymentCallbackOutcome.UnknownOrder:
                return RedirectToAction("Index", "Home");

            case PaymentCallbackOutcome.Paid:
                TempData["Success"] = "Ödemeniz alındı, siparişiniz oluşturuldu.";
                break;

            case PaymentCallbackOutcome.Failed:
                TempData["Error"] = "Ödemeniz tamamlanamadı ya da doğrulanamadı. \"Ödemeyi Tamamla\" düğmesiyle tekrar deneyebilirsiniz; ödeme daha önce alındıysa siparişiniz otomatik onaylanır ve tekrar çekim yapılmaz.";
                break;
        }

        return RedirectToAction(nameof(Details), new { id = result.OrderId });
    }

    private string GetClientIp()
    {
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip is null || IPAddress.IsLoopback(ip))
        {
            return "127.0.0.1";
        }

        return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
