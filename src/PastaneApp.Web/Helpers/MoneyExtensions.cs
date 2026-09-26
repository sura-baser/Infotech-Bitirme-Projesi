using System.Globalization;

namespace PastaneApp.Web.Helpers;

public static class MoneyExtensions
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    // Sunucunun dil ayarından bağımsız olarak fiyatı Türk lirası olarak gösterir (örn. ₺2.000,00).
    public static string ToTl(this decimal amount) => amount.ToString("C2", Turkish);
}
