using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Web.Models.PartyBox;

namespace PastaneApp.Web.Controllers;

public class PartyBoxController : Controller
{
    private static readonly string[] FlavorProductNames = { "Cookies", "Muffins", "Tiramisu Balls", "Cinnamon Roll" };

    private readonly IUnitOfWork _unitOfWork;

    public PartyBoxController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var boxCategory = (await _unitOfWork.Repository<Category>().FindAsync(c => c.Name == "Parti Kutusu")).FirstOrDefault();

        var sizeProducts = boxCategory is null
            ? new List<Product>()
            : (await _unitOfWork.Repository<Product>().FindAsync(p => p.CategoryId == boxCategory.Id && p.IsActive))
                .OrderBy(p => p.Price)
                .ToList();

        var flavorProducts = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Images);

        var flavors = flavorProducts
            .Where(p => FlavorProductNames.Contains(p.Name))
            .SelectMany(p => p.Images
                .Where(i => i.ImageType == ImageType.Finished)
                .OrderBy(i => i.SortOrder)
                .Select(i => new PartyBoxFlavorOption
                {
                    Label = string.IsNullOrWhiteSpace(i.Label) ? p.Name : i.Label,
                    ImageUrl = i.ImageUrl
                }))
            .ToList();

        var model = new PartyBoxIndexViewModel
        {
            Sizes = sizeProducts.Select(p => new PartyBoxSizeOption
            {
                ProductId = p.Id,
                Name = p.Name,
                PieceCount = ParsePieceCount(p.ServingInfo),
                Price = p.Price
            }).ToList(),
            Flavors = flavors
        };

        return View(model);
    }

    internal static int ParsePieceCount(string? servingInfo)
    {
        if (string.IsNullOrWhiteSpace(servingInfo))
        {
            return 0;
        }

        var digits = new string(servingInfo.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var count) ? count : 0;
    }
}
