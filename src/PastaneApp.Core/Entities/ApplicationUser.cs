using Microsoft.AspNetCore.Identity;

namespace PastaneApp.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Address { get; set; }
}
