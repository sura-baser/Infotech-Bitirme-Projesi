using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Api.Dtos;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [StringLength(100)] string? FullName);

public record LoginResponse(string Token, DateTime ExpiresAtUtc, string Email, IReadOnlyList<string> Roles);
