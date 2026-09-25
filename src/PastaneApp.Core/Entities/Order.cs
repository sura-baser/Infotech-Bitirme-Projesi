using PastaneApp.Core.Enums;

namespace PastaneApp.Core.Entities;

public class Order : BaseEntity
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentToken { get; set; }
    public string? PaymentId { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
