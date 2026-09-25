using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Web.Models.Orders;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
    [StringLength(500)]
    [Display(Name = "Teslimat Adresi")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Şehir")]
    public string DeliveryCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [StringLength(20)]
    [Display(Name = "Telefon Numarası")]
    public string PhoneNumber { get; set; } = string.Empty;
}
