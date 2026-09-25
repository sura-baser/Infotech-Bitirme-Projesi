using PastaneApp.Core.Entities;

namespace PastaneApp.Web.Models.Orders;

public class PayViewModel
{
    public Order Order { get; set; } = null!;
    public string CheckoutFormHtml { get; set; } = string.Empty;
}
