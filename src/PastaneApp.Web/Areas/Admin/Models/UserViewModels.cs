using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Web.Areas.Admin.Models;

public class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public bool IsLocked { get; set; }
    public bool IsCurrentUser { get; set; }
}

public class UserFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Ad Soyad")]
    public string? FullName { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string? Password { get; set; }

    [Required]
    [Display(Name = "Rol")]
    public string Role { get; set; } = "Customer";

    [Display(Name = "Hesap kilitli")]
    public bool IsLocked { get; set; }

    public bool IsEdit => !string.IsNullOrEmpty(Id);
    public bool IsCurrentUser { get; set; }
}

public class UserDeleteViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public bool IsCurrentUser { get; set; }
}
