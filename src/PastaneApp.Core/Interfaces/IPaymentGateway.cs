using PastaneApp.Core.Payments;

namespace PastaneApp.Core.Interfaces;

public interface IPaymentGateway
{
    bool IsConfigured { get; }

    Task<PaymentInitResult> InitializeAsync(PaymentInitRequest request);

    Task<PaymentVerifyResult> VerifyAsync(string token);
}
