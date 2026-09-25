namespace PastaneApp.Core.Payments;

public record PaymentBasketItem(string Id, string Name, decimal Price);

public record PaymentInitRequest(
    int OrderId,
    decimal TotalAmount,
    string CallbackUrl,
    string BuyerId,
    string BuyerName,
    string BuyerSurname,
    string BuyerEmail,
    string BuyerPhone,
    string BuyerIp,
    string Address,
    string City,
    IReadOnlyList<PaymentBasketItem> Items);

public record PaymentInitResult(bool Success, string? Token, string? CheckoutFormHtml, string? ErrorMessage);

public record PaymentVerifyResult(bool Success, int? OrderId, decimal PaidAmount, string? PaymentId, string? ErrorMessage);
