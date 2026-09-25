using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;

namespace PastaneApp.Core.Services;

public record DeliveryInfo(string Address, string City, string Phone);

public record PaymentBuyer(string? FullName, string? Email);

public record CreateOrderResult(bool Success, int? OrderId, bool CartEmpty, string? Error)
{
    public static CreateOrderResult Created(int orderId) => new(true, orderId, false, null);
    public static CreateOrderResult Empty() => new(false, null, true, null);
    public static CreateOrderResult Failed(string error) => new(false, null, false, error);
}

public enum PaymentStartOutcome
{
    NotFound,
    AlreadyPaid,
    Unavailable,
    CompletedEarlier,
    FormReady
}

public record PaymentStartResult(PaymentStartOutcome Outcome, Order? Order = null, string? CheckoutFormHtml = null, string? Error = null);

public enum PaymentCallbackOutcome
{
    UnknownOrder,
    AlreadyPaid,
    Paid,
    Failed
}

public record PaymentCallbackResult(PaymentCallbackOutcome Outcome, int? OrderId = null);

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetUserOrdersAsync(string userId);
    Task<Order?> GetUserOrderAsync(string userId, int orderId);
    Task<CreateOrderResult> CreateOrderFromCartAsync(string userId, DeliveryInfo delivery);
    Task<PaymentStartResult> StartPaymentAsync(string userId, int orderId, PaymentBuyer buyer, string callbackUrl, string clientIp);
    Task<PaymentCallbackResult> CompletePaymentAsync(string? token);

    Task<IReadOnlyList<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderWithCustomerAsync(int orderId);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus status);
}
