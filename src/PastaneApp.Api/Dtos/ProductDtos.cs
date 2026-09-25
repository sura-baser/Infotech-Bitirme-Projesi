using System.ComponentModel.DataAnnotations;

namespace PastaneApp.Api.Dtos;

public record ProductDto(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<string> Highlights,
    decimal Price,
    int Stock,
    string? ServingInfo,
    bool IsActive,
    int CategoryId,
    string CategoryName,
    string? ImageUrl,
    IReadOnlyList<string> Allergens);

public record ProductUpsertRequest(
    [Required, StringLength(150)] string Name,
    [StringLength(2000)] string? Description,
    string? Highlights,
    [Range(0.01, 1000000)] decimal Price,
    [Range(0, 100000)] int Stock,
    [StringLength(100)] string? ServingInfo,
    bool IsActive,
    [Range(1, int.MaxValue)] int CategoryId);
