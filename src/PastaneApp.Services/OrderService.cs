using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Core.Payments;
using PastaneApp.Core.Services;

namespace PastaneApp.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public OrderService(IUnitOfWork unitOfWork, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<IReadOnlyList<Order>> GetUserOrdersAsync(string userId)
    {
        var orders = await _unitOfWork.Repository<Order>().FindAsync(o => o.ApplicationUserId == userId);
        return orders.OrderByDescending(o => o.OrderDate).ToList();
    }

    public async Task<Order?> GetUserOrderAsync(string userId, int orderId)
    {
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId, o => o.OrderDetails);
        return order is not null && order.ApplicationUserId == userId ? order : null;
    }

    public async Task<CreateOrderResult> CreateOrderFromCartAsync(string userId, DeliveryInfo delivery)
    {
        var cart = await GetCartWithItemsAsync(userId);
        if (cart is null || !cart.CartItems.Any())
        {
            return CreateOrderResult.Empty();
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
            return CreateOrderResult.Failed($"Şu ürünlerde yeterli stok yok: {string.Join(", ", outOfStock)}. Lütfen sepetinizi güncelleyin.");
        }

        var order = new Order
        {
            ApplicationUserId = userId,
            DeliveryAddress = delivery.Address.Trim(),
            DeliveryCity = delivery.City.Trim(),
            PhoneNumber = delivery.Phone.Trim(),
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

        return CreateOrderResult.Created(order.Id);
    }

    public async Task<PaymentStartResult> StartPaymentAsync(string userId, int orderId, PaymentBuyer buyer, string callbackUrl, string clientIp)
    {
        var order = await GetUserOrderAsync(userId, orderId);
        if (order is null)
        {
            return new PaymentStartResult(PaymentStartOutcome.NotFound);
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return new PaymentStartResult(PaymentStartOutcome.AlreadyPaid, order);
        }

        if (!_paymentGateway.IsConfigured)
        {
            return new PaymentStartResult(PaymentStartOutcome.Unavailable, order, Error: "Ödeme sistemi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.");
        }

        if (!string.IsNullOrEmpty(order.PaymentToken))
        {
            var previous = await _paymentGateway.VerifyAsync(order.PaymentToken);
            if (IsValidPayment(order, previous))
            {
                await FinalizePaymentAsync(order, previous);
                return new PaymentStartResult(PaymentStartOutcome.CompletedEarlier, order);
            }
        }

        var (name, surname) = SplitName(buyer.FullName, buyer.Email);
        var request = new PaymentInitRequest(
            order.Id,
            order.TotalAmount,
            callbackUrl,
            order.ApplicationUserId,
            name,
            surname,
            buyer.Email ?? string.Empty,
            NormalizePhone(order.PhoneNumber),
            clientIp,
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
            return new PaymentStartResult(PaymentStartOutcome.Unavailable, order, Error: "Ödeme formu açılamadı, lütfen tekrar deneyin.");
        }

        order.PaymentToken = result.Token;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();

        return new PaymentStartResult(PaymentStartOutcome.FormReady, order, result.CheckoutFormHtml);
    }

    public async Task<PaymentCallbackResult> CompletePaymentAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new PaymentCallbackResult(PaymentCallbackOutcome.UnknownOrder);
        }

        var match = (await _unitOfWork.Repository<Order>().FindAsync(o => o.PaymentToken == token)).FirstOrDefault();
        var order = match is null
            ? null
            : await _unitOfWork.Repository<Order>().GetByIdAsync(match.Id, o => o.OrderDetails);
        if (order is null)
        {
            return new PaymentCallbackResult(PaymentCallbackOutcome.UnknownOrder);
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return new PaymentCallbackResult(PaymentCallbackOutcome.AlreadyPaid, order.Id);
        }

        var result = await _paymentGateway.VerifyAsync(token);
        if (IsValidPayment(order, result))
        {
            await FinalizePaymentAsync(order, result);
            return new PaymentCallbackResult(PaymentCallbackOutcome.Paid, order.Id);
        }

        order.PaymentStatus = PaymentStatus.Failed;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();
        return new PaymentCallbackResult(PaymentCallbackOutcome.Failed, order.Id);
    }

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync()
    {
        var orders = await _unitOfWork.Repository<Order>().GetAllAsync(o => o.ApplicationUser);
        return orders.OrderByDescending(o => o.OrderDate).ToList();
    }

    public Task<Order?> GetOrderWithCustomerAsync(int orderId) =>
        _unitOfWork.Repository<Order>().GetByIdAsync(orderId, o => o.ApplicationUser, o => o.OrderDetails);

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
        if (order is null)
        {
            return false;
        }

        order.Status = status;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();
        return true;
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

        var cart = await GetCartWithItemsAsync(order.ApplicationUserId);
        if (cart is not null)
        {
            foreach (var item in cart.CartItems)
            {
                _unitOfWork.Repository<CartItem>().Remove(item);
            }
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task<Cart?> GetCartWithItemsAsync(string userId)
    {
        var cart = (await _unitOfWork.Repository<Cart>().FindAsync(c => c.ApplicationUserId == userId)).FirstOrDefault();
        return cart is null
            ? null
            : await _unitOfWork.Repository<Cart>().GetByIdAsync(cart.Id, c => c.CartItems);
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
}
