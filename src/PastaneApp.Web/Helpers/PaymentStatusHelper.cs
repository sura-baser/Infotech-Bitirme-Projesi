using PastaneApp.Core.Enums;

namespace PastaneApp.Web.Helpers;

public static class PaymentStatusHelper
{
    public static string GetDisplayName(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Ödeme Bekliyor",
        PaymentStatus.Paid => "Ödendi",
        PaymentStatus.Failed => "Ödeme Başarısız",
        _ => status.ToString()
    };

    public static string GetBadgeStyle(PaymentStatus status) => status switch
    {
        PaymentStatus.Paid => "background:var(--ok-bg); color:var(--ok);",
        _ => "background:var(--off-bg); color:var(--off);"
    };
}
