using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Web.Areas.Admin.Models;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}
