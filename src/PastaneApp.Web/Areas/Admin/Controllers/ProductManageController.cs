using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Areas.Admin.Models;

namespace PastaneApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductManageController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductManageController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Manage(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, p => p.Images, p => p.Ingredients);
        if (product is null)
        {
            return NotFound();
        }

        var allAllergens = await _unitOfWork.Repository<Allergen>().GetAllAsync();
        var productAllergens = await _unitOfWork.Repository<ProductAllergen>().FindAsync(pa => pa.ProductId == id);

        var model = new ProductManageViewModel
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Images = product.Images.OrderBy(i => i.ImageType).ThenBy(i => i.SortOrder).ToList(),
            Ingredients = product.Ingredients.OrderBy(i => i.SortOrder).ToList(),
            AllAllergens = allAllergens.OrderBy(a => a.Name).ToList(),
            SelectedAllergenIds = productAllergens.Select(pa => pa.AllergenId).ToHashSet()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage(int productId, string imageUrl, ImageType imageType, int sortOrder)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            await _unitOfWork.Repository<ProductImage>().AddAsync(new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl.Trim(),
                ImageType = imageType,
                SortOrder = sortOrder
            });
            await _unitOfWork.CompleteAsync();
        }

        return RedirectToAction(nameof(Manage), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int imageId, int productId)
    {
        var image = await _unitOfWork.Repository<ProductImage>().GetByIdAsync(imageId);
        if (image is not null)
        {
            _unitOfWork.Repository<ProductImage>().Remove(image);
            await _unitOfWork.CompleteAsync();
        }

        return RedirectToAction(nameof(Manage), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddIngredient(int productId, string name, int sortOrder)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _unitOfWork.Repository<ProductIngredient>().AddAsync(new ProductIngredient
            {
                ProductId = productId,
                Name = name.Trim(),
                SortOrder = sortOrder
            });
            await _unitOfWork.CompleteAsync();
        }

        return RedirectToAction(nameof(Manage), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIngredient(int ingredientId, int productId)
    {
        var ingredient = await _unitOfWork.Repository<ProductIngredient>().GetByIdAsync(ingredientId);
        if (ingredient is not null)
        {
            _unitOfWork.Repository<ProductIngredient>().Remove(ingredient);
            await _unitOfWork.CompleteAsync();
        }

        return RedirectToAction(nameof(Manage), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAllergens(int productId, List<int>? allergenIds)
    {
        allergenIds ??= new List<int>();

        var existing = await _unitOfWork.Repository<ProductAllergen>().FindAsync(pa => pa.ProductId == productId);
        var repo = _unitOfWork.Repository<ProductAllergen>();

        foreach (var pa in existing.Where(pa => !allergenIds.Contains(pa.AllergenId)))
        {
            repo.Remove(pa);
        }

        var existingAllergenIds = existing.Select(pa => pa.AllergenId).ToHashSet();
        foreach (var allergenId in allergenIds.Where(id => !existingAllergenIds.Contains(id)))
        {
            await repo.AddAsync(new ProductAllergen { ProductId = productId, AllergenId = allergenId });
        }

        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Alerjen bilgileri güncellendi.";
        return RedirectToAction(nameof(Manage), new { id = productId });
    }
}
