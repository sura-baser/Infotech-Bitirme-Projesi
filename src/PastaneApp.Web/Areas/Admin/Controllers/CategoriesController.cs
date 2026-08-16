using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Areas.Admin.Models;

namespace PastaneApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View(new CategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = new Category { Name = model.Name, Description = model.Description };
        await _unitOfWork.Repository<Category>().AddAsync(category);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Kategori oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        var model = new CategoryViewModel { Id = category.Id, Name = category.Name, Description = category.Description };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = model.Name;
        category.Description = model.Description;
        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Kategori güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        _unitOfWork.Repository<Category>().Remove(category);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }
}
