using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Models.Products;

namespace PastaneApp.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search, int[]? excludeAllergenIds)
    {
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Category, p => p.Images);
        var productAllergens = await _unitOfWork.Repository<ProductAllergen>().GetAllAsync();
        var allergens = await _unitOfWork.Repository<Allergen>().GetAllAsync();
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();

        var allergenLookup = productAllergens
            .GroupBy(pa => pa.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(pa => pa.AllergenId).ToHashSet());

        IEnumerable<Product> query = products.Where(p => p.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var excluded = (excludeAllergenIds ?? Array.Empty<int>()).ToHashSet();
        if (excluded.Count > 0)
        {
            query = query.Where(p => !allergenLookup.TryGetValue(p.Id, out var ids) || !ids.Overlaps(excluded));
        }

        var cards = query.Select(p => new ProductCardViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryName = p.Category?.Name ?? string.Empty,
            ImageUrl = p.Images
                .Where(i => i.ImageType == ImageType.Finished)
                .OrderBy(i => i.SortOrder)
                .Select(i => i.ImageUrl)
                .FirstOrDefault()
        }).ToList();

        var model = new ProductListViewModel
        {
            Products = cards,
            Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
            AllAllergens = allergens.OrderBy(a => a.Name).ToList(),
            SelectedCategoryId = categoryId,
            ExcludedAllergenIds = excluded.ToList(),
            Search = search
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, p => p.Category, p => p.Images, p => p.Ingredients);
        if (product is null || !product.IsActive)
        {
            return NotFound();
        }

        var productAllergens = await _unitOfWork.Repository<ProductAllergen>().FindAsync(pa => pa.ProductId == id);
        var allergenIds = productAllergens.Select(pa => pa.AllergenId).ToHashSet();

        var allergenNames = allergenIds.Count == 0
            ? new List<string>()
            : (await _unitOfWork.Repository<Allergen>().GetAllAsync())
                .Where(a => allergenIds.Contains(a.Id))
                .Select(a => a.Name)
                .ToList();

        var model = new ProductDetailViewModel
        {
            Product = product,
            AllergenNames = allergenNames
        };

        return View(model);
    }
}
