using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Api.Dtos;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;

namespace PastaneApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] int[]? excludeAllergenIds)
    {
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Category, p => p.Images);
        var allergenNames = await GetAllergenNamesByProductAsync();

        var isAdmin = User.IsInRole("Admin");
        IEnumerable<Product> query = products.Where(p => isAdmin || IsPubliclyVisible(p));

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (excludeAllergenIds is { Length: > 0 })
        {
            var productAllergens = await _unitOfWork.Repository<ProductAllergen>().GetAllAsync();
            var excluded = excludeAllergenIds.ToHashSet();
            var blockedProductIds = productAllergens
                .Where(pa => excluded.Contains(pa.AllergenId))
                .Select(pa => pa.ProductId)
                .ToHashSet();

            query = query.Where(p => !blockedProductIds.Contains(p.Id));
        }

        return query
            .OrderBy(p => p.Id)
            .Select(p => ToDto(p, allergenNames.GetValueOrDefault(p.Id, new List<string>())))
            .ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, p => p.Category, p => p.Images);
        if (product is null || (!IsPubliclyVisible(product) && !User.IsInRole("Admin")))
        {
            return NotFound();
        }

        var allergenNames = await GetAllergenNamesByProductAsync();
        return ToDto(product, allergenNames.GetValueOrDefault(product.Id, new List<string>()));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertRequest request)
    {
        if (await _unitOfWork.Repository<Category>().GetByIdAsync(request.CategoryId) is null)
        {
            return BadRequest(new ProblemDetails { Title = "Belirtilen kategori bulunamadı." });
        }

        var product = new Product();
        Apply(product, request);

        await _unitOfWork.Repository<Product>().AddAsync(product);
        await _unitOfWork.CompleteAsync();

        var created = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id, p => p.Category, p => p.Images);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToDto(created!, new List<string>()));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ProductUpsertRequest request)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        if (await _unitOfWork.Repository<Category>().GetByIdAsync(request.CategoryId) is null)
        {
            return BadRequest(new ProblemDetails { Title = "Belirtilen kategori bulunamadı." });
        }

        Apply(product, request);

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var usedInOrders = (await _unitOfWork.Repository<OrderDetail>().FindAsync(od => od.ProductId == id)).Any();
        if (usedInOrders)
        {
            return Conflict(new ProblemDetails { Title = "Bu ürün sipariş geçmişinde kullanıldığı için silinemez. Bunun yerine IsActive=false yaparak satıştan kaldırın." });
        }

        var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.ProductId == id);
        foreach (var cartItem in cartItems)
        {
            _unitOfWork.Repository<CartItem>().Remove(cartItem);
        }

        _unitOfWork.Repository<Product>().Remove(product);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private static bool IsPubliclyVisible(Product product) => product.IsActive && product.Category.ShowOnHome;

    private static void Apply(Product product, ProductUpsertRequest request)
    {
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Highlights = request.Highlights?.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.ServingInfo = request.ServingInfo?.Trim();
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
    }

    private async Task<Dictionary<int, List<string>>> GetAllergenNamesByProductAsync()
    {
        var productAllergens = await _unitOfWork.Repository<ProductAllergen>().GetAllAsync();
        var allergens = (await _unitOfWork.Repository<Allergen>().GetAllAsync()).ToDictionary(a => a.Id, a => a.Name);

        return productAllergens
            .Where(pa => allergens.ContainsKey(pa.AllergenId))
            .GroupBy(pa => pa.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(pa => allergens[pa.AllergenId]).OrderBy(n => n).ToList());
    }

    private static ProductDto ToDto(Product product, IReadOnlyList<string> allergens) => new(
        product.Id,
        product.Name,
        product.Description,
        (product.Highlights ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        product.Price,
        product.Stock,
        product.ServingInfo,
        product.IsActive,
        product.CategoryId,
        product.Category?.Name ?? string.Empty,
        product.Images
            .Where(i => i.ImageType == ImageType.Finished)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault(),
        allergens);
}
