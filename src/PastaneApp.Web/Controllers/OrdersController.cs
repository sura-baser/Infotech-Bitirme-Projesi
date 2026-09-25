using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Core.Payments;
using PastaneApp.Web.Models.Orders;

namespace PastaneApp.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentGateway _paymentGateway;

    public OrdersController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _paymentGateway = paymentGateway;
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

        var outOfStock = cart.CartItems
            .Where(item => products.TryGetValue(item.ProductId, out var p) && item.Quantity > p.Stock)
            .Select(item => products[item.ProductId].Name)
            .ToList();
        if (outOfStock.Count > 0)
        {
            TempData["Error"] = $"Şu ürünlerde yeterli stok yok: {string.Join(", ", outOfStock)}. Lütfen sepetinizi güncelleyin.";
            return RedirectToAction("Index", "Cart");
        }

        var order = new Order
        {
            ApplicationUserId = GetUserId(),
            DeliveryAddress = model.DeliveryAddress.Trim(),
            DeliveryCity = model.DeliveryCity.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending
        };

        foreach (var item in cart.CartItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                continue;
            }

            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                CustomizationNotes = item.CustomizationNotes
            });
        }

        order.TotalAmount = order.OrderDetails.Sum(d => d.UnitPrice * d.Quantity);

        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.CompleteAsync();

        return RedirectToAction(nameof(Pay), new { id = order.Id });
    }

    public async Task<IActionResult> Pay(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id, o => o.OrderDetails);
        if (order is null || order.ApplicationUserId != GetUserId())
        {
            return NotFound();
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!_paymentGateway.IsConfigured)
        {
            TempData["Error"] = "Ödeme sistemi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!string.IsNullOrEmpty(order.PaymentToken))
        {
            var previous = await _paymentGateway.VerifyAsync(order.PaymentToken);
            if (IsValidPayment(order, previous))
            {
                await FinalizePaymentAsync(order, previous);
                TempData["Success"] = "Ödemeniz alındı, siparişiniz oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        var user = await _userManager.GetUserAsync(User);
        var (name, surname) = SplitName(user?.FullName, user?.Email);

        var request = new PaymentInitRequest(
            order.Id,
            order.TotalAmount,
            Url.Action(nameof(PaymentCallback), "Orders", null, Request.Scheme)!,
            order.ApplicationUserId,
            name,
            surname,
            user?.Email ?? string.Empty,
            NormalizePhone(order.PhoneNumber),
            GetClientIp(),
            order.DeliveryAddress,
            order.DeliveryCity,
            order.OrderDetails
                .Select(d => new PaymentBasketItem(
                    d.Id.ToString(),
                    d.Quantity > 1 ? $"{d.ProductName} x{d.Quantity}" : d.ProductName,
                    d.UnitPrice * d.Quantity))
                .ToList());

        var result = await _paymentGateway.InitializeAsync(request);
        if (!result.Success || string.IsNullOrEmpty(result.CheckoutFormHtml))
        {
            TempData["Error"] = "Ödeme formu açılamadı, lütfen tekrar deneyin.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.PaymentToken = result.Token;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();

        return View(new PayViewModel { Order = order, CheckoutFormHtml = result.CheckoutFormHtml });
    }

    // iyzico ödeme sonucunu kullanıcının tarayıcısı üzerinden başka bir siteden POST eder,
    // bu yüzden oturum çerezi ve antiforgery jetonu yok; siparişi ödeme jetonundan buluruz.
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PaymentCallback([FromForm] string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Index", "Home");
        }

        var match = (await _unitOfWork.Repository<Order>().FindAsync(o => o.PaymentToken == token)).FirstOrDefault();
        if (match is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(match.Id, o => o.OrderDetails);
        if (order is null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        var result = await _paymentGateway.VerifyAsync(token);
        if (IsValidPayment(order, result))
        {
            await FinalizePaymentAsync(order, result);
            TempData["Success"] = "Ödemeniz alındı, siparişiniz oluşturuldu.";
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Failed;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            TempData["Error"] = "Ödemeniz tamamlanamadı ya da doğrulanamadı. \"Ödemeyi Tamamla\" düğmesiyle tekrar deneyebilirsiniz; ödeme daha önce alındıysa siparişiniz otomatik onaylanır ve tekrar çekim yapılmaz.";
        }

        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    private static bool IsValidPayment(Order order, PaymentVerifyResult result) =>
        result.Success && result.OrderId == order.Id && result.PaidAmount >= order.TotalAmount;

    private async Task FinalizePaymentAsync(Order order, PaymentVerifyResult result)
    {
        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return;
        }

        order.PaymentStatus = PaymentStatus.Paid;
        order.PaymentId = result.PaymentId;
        order.PaidAt = DateTime.UtcNow;
        _unitOfWork.Repository<Order>().Update(order);

        var productIds = order.OrderDetails.Select(d => d.ProductId).ToHashSet();
        var products = (await _unitOfWork.Repository<Product>().GetAllAsync())
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id);

        foreach (var detail in order.OrderDetails)
        {
            if (products.TryGetValue(detail.ProductId, out var product))
            {
                product.Stock = Math.Max(0, product.Stock - detail.Quantity);
                _unitOfWork.Repository<Product>().Update(product);
            }
        }

        var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.ApplicationUserId == order.ApplicationUserId);
        var cart = carts.FirstOrDefault();
        if (cart is not null)
        {
            var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartId == cart.Id);
            foreach (var item in cartItems)
            {
                _unitOfWork.Repository<CartItem>().Remove(item);
            }
        }

        await _unitOfWork.CompleteAsync();
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

    private static (string Name, string Surname) SplitName(string? fullName, string? email)
    {
        var parts = (fullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => (EmailLocalPart(email), EmailLocalPart(email)),
            1 => (parts[0], parts[0]),
            _ => (string.Join(' ', parts[..^1]), parts[^1])
        };
    }

    private static string EmailLocalPart(string? email)
    {
        var local = (email ?? string.Empty).Split('@')[0];
        return string.IsNullOrWhiteSpace(local) ? "Müşteri" : local;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits switch
        {
            { Length: 11 } when digits.StartsWith('0') => "+90" + digits[1..],
            { Length: 12 } when digits.StartsWith("90") => "+" + digits,
            { Length: 10 } => "+90" + digits,
            _ => phone
        };
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
