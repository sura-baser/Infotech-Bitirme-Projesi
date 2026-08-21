using PastaneApp.Core.Enums;

namespace PastaneApp.Web.Helpers;

public static class OrderStatusHelper
{
    public static string GetDisplayName(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Beklemede",
        OrderStatus.Preparing => "Hazırlanıyor",
        OrderStatus.OutForDelivery => "Yolda",
        OrderStatus.Delivered => "Teslim Edildi",
        OrderStatus.Cancelled => "İptal Edildi",
        _ => status.ToString()
    };
}
