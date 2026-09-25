using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Api.Dtos;

public record CategoryDto(int Id, string Name, string? Description);

public record CategoryUpsertRequest(
    [Required, StringLength(100)] string Name,
    [StringLength(500)] string? Description);
