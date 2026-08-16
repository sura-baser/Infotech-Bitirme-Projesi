using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Areas.Admin.Models;

namespace PastaneApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Category);
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        var model = new ProductViewModel { Categories = await GetCategorySelectListAsync() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategorySelectListAsync();
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Stock = model.Stock,
            ServingInfo = model.ServingInfo,
            IsActive = model.IsActive,
            CategoryId = model.CategoryId
        };

        await _unitOfWork.Repository<Product>().AddAsync(product);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Ürün oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ServingInfo = product.ServingInfo,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            Categories = await GetCategorySelectListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategorySelectListAsync();
            return View(model);
        }

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.Stock = model.Stock;
        product.ServingInfo = model.ServingInfo;
        product.IsActive = model.IsActive;
        product.CategoryId = model.CategoryId;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Ürün güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, p => p.Category);
        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _unitOfWork.Repository<Product>().Remove(product);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Ürün silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategorySelectListAsync()
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        return categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
    }
}
