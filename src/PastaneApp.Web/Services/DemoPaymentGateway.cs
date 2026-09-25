using System.Globalization;
using System.Net;
using PastaneApp.Core.Interfaces;
using PastaneApp.Core.Payments;

namespace PastaneApp.Web.Services;

// Yalnızca geliştirme ortamında ve iyzico anahtarı yokken kullanılır (bkz. Program.cs).
// Gerçek tahsilat yapmaz; jeton sipariş numarasını ve tutarı taşır.
public class DemoPaymentGateway : IPaymentGateway
{
    private const string TokenPrefix = "demo";

    public bool IsConfigured => true;

    public Task<PaymentInitResult> InitializeAsync(PaymentInitRequest request)
    {
        var amount = request.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture);
        var token = $"{TokenPrefix}|{request.OrderId}|{amount}|{Guid.NewGuid():N}";

        var html = $"""
            <div style="border:1px dashed var(--line); background:#fff; padding:24px;">
                <div style="font-size:13px; letter-spacing:.08em; text-transform:uppercase; color:var(--ink-soft); font-weight:600; margin-bottom:10px;">Demo Ödeme</div>
                <p style="font-size:14px; line-height:1.7; color:var(--ink-soft); margin:0 0 18px;">Bu ortam yalnızca geliştirme içindir, kartınızdan gerçek bir çekim yapılmaz. Ödemeyi tamamlamak için düğmeye basın.</p>
                <form method="post" action="{WebUtility.HtmlEncode(request.CallbackUrl)}">
                    <input type="hidden" name="token" value="{WebUtility.HtmlEncode(token)}" />
                    <button type="submit" class="as-pill">Demo Ödemeyi Tamamla</button>
                </form>
            </div>
            """;

        return Task.FromResult(new PaymentInitResult(true, token, html, null));
    }

    public Task<PaymentVerifyResult> VerifyAsync(string token)
    {
        var parts = token.Split('|');
        if (parts.Length == 4
            && parts[0] == TokenPrefix
            && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var orderId)
            && decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return Task.FromResult(new PaymentVerifyResult(true, orderId, amount, $"DEMO-{orderId}", null));
        }

        return Task.FromResult(new PaymentVerifyResult(false, null, 0, null, "Geçersiz demo ödeme jetonu."));
    }
}
