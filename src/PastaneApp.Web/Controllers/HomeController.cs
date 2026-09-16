using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Models;
using PastaneApp.Web.Models.Products;

namespace PastaneApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Category, p => p.Images);

        var model = new HomeIndexViewModel
        {
            Categories = categories.Where(c => c.ShowOnHome).Select(c => new CategoryCardViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = products
                    .Where(p => p.CategoryId == c.Id)
                    .SelectMany(p => p.Images)
                    .Where(i => i.ImageType == ImageType.Finished)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            }).ToList(),
            FeaturedProducts = products
                .Where(p => p.IsActive && (p.Category?.ShowOnHome ?? false))
                .Take(3)
                .Select(p => new ProductCardViewModel
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
                })
                .ToList()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
