using System.Globalization;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;
using PastaneApp.Core.Interfaces;
using PastaneApp.Core.Payments;

namespace PastaneApp.Web.Services;

public class IyzicoPaymentGateway : IPaymentGateway
{
    private const string TurkishLira = "TRY";

    private readonly IyzicoOptions _options;
    private readonly ILogger<IyzicoPaymentGateway> _logger;

    public IyzicoPaymentGateway(IOptions<IyzicoOptions> options, ILogger<IyzicoPaymentGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiKey) && !string.IsNullOrWhiteSpace(_options.SecretKey);

    public async Task<PaymentInitResult> InitializeAsync(PaymentInitRequest request)
    {
        var buyerFullName = $"{request.BuyerName} {request.BuyerSurname}";
        var address = new Address
        {
            ContactName = buyerFullName,
            City = request.City,
            Country = "Turkey",
            Description = request.Address
        };

        var checkoutRequest = new CreateCheckoutFormInitializeRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = request.OrderId.ToString(CultureInfo.InvariantCulture),
            Price = FormatAmount(request.TotalAmount),
            PaidPrice = FormatAmount(request.TotalAmount),
            Currency = TurkishLira,
            BasketId = $"B{request.OrderId}",
            PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl = request.CallbackUrl,
            Buyer = new Buyer
            {
                Id = request.BuyerId,
                Name = request.BuyerName,
                Surname = request.BuyerSurname,
                GsmNumber = request.BuyerPhone,
                Email = request.BuyerEmail,
                // iyzico bu alanı zorunlu tutar, T.C. kimlik numarası toplamıyoruz.
                IdentityNumber = "11111111111",
                RegistrationAddress = request.Address,
                Ip = request.BuyerIp,
                City = request.City,
                Country = "Turkey"
            },
            ShippingAddress = address,
            BillingAddress = address,
            BasketItems = request.Items.Select(item => new BasketItem
            {
                Id = item.Id,
                Name = item.Name,
                Category1 = "Tatlı",
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Price = FormatAmount(item.Price)
            }).ToList()
        };

        try
        {
            var result = await Task.Run(() => CheckoutFormInitialize.Create(checkoutRequest, BuildOptions()));
            if (!IsSuccess(result.Status))
            {
                _logger.LogWarning("iyzico ödeme formu oluşturulamadı: {Code} {Message}", result.ErrorCode, result.ErrorMessage);
                return new PaymentInitResult(false, null, null, result.ErrorMessage);
            }

            return new PaymentInitResult(true, result.Token, result.CheckoutFormContent, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iyzico ödeme formu isteği başarısız oldu.");
            return new PaymentInitResult(false, null, null, "Ödeme sistemine ulaşılamadı.");
        }
    }

    public async Task<PaymentVerifyResult> VerifyAsync(string token)
    {
        var retrieveRequest = new RetrieveCheckoutFormRequest
        {
            Locale = Locale.TR.ToString(),
            Token = token
        };

        try
        {
            var result = await Task.Run(() => CheckoutForm.Retrieve(retrieveRequest, BuildOptions()));

            var paid = IsSuccess(result.Status)
                && string.Equals(result.PaymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase);
            if (!paid)
            {
                _logger.LogInformation("iyzico ödeme sonucu başarısız: {Status} {PaymentStatus} {Message}", result.Status, result.PaymentStatus, result.ErrorMessage);
                return new PaymentVerifyResult(false, null, 0, null, result.ErrorMessage);
            }

            int? orderId = int.TryParse(result.ConversationId, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;
            var paidAmount = decimal.TryParse(result.PaidPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0m;

            return new PaymentVerifyResult(true, orderId, paidAmount, result.PaymentId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iyzico ödeme sonucu sorgulanamadı.");
            return new PaymentVerifyResult(false, null, 0, null, "Ödeme sonucu doğrulanamadı.");
        }
    }

    private Iyzipay.Options BuildOptions() => new()
    {
        ApiKey = _options.ApiKey,
        SecretKey = _options.SecretKey,
        BaseUrl = _options.BaseUrl
    };

    private static bool IsSuccess(string? status) =>
        string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);

    private static string FormatAmount(decimal amount) =>
        amount.ToString("0.00", CultureInfo.InvariantCulture);
}
