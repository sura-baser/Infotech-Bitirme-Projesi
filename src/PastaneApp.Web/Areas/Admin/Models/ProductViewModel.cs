using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PastaneApp.Web.Areas.Admin.Models;

public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, 100000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    [Display(Name = "Fiyat (₺)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stok zorunludur.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
    [Display(Name = "Stok")]
    public int Stock { get; set; }

    [StringLength(100)]
    [Display(Name = "Porsiyon Bilgisi")]
    public string? ServingInfo { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}
