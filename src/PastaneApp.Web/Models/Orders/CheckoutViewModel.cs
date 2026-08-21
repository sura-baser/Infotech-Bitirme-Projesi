using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Web.Models.Orders;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
    [StringLength(500)]
    [Display(Name = "Teslimat Adresi")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [StringLength(20)]
    [Display(Name = "Telefon Numarası")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kart numarası zorunludur.")]
    [StringLength(19, MinimumLength = 12, ErrorMessage = "Geçerli bir kart numarası girin.")]
    [Display(Name = "Kart Numarası")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Son kullanma tarihi zorunludur.")]
    [Display(Name = "Son Kullanma Tarihi (AA/YY)")]
    public string CardExpiry { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV zorunludur.")]
    [StringLength(4, MinimumLength = 3)]
    [Display(Name = "CVV")]
    public string CardCvv { get; set; } = string.Empty;
}
