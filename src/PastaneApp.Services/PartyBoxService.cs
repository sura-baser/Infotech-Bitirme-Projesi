using PastaneApp.Core.Entities;
using PastaneApp.Core.Enums;
using PastaneApp.Core.Interfaces;
using PastaneApp.Core.Services;

namespace PastaneApp.Services;

public class PartyBoxService : IPartyBoxService
{
    private static readonly string[] FlavorProductNames = { "Cookies", "Muffins", "Tiramisu Balls", "Cinnamon Roll" };

    private readonly IUnitOfWork _unitOfWork;

    public PartyBoxService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PartyBoxOptions> GetOptionsAsync()
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
                .Where(i => i.ImageType == ImageType.Process)
                .OrderBy(i => i.SortOrder)
                .Select(i => new PartyBoxFlavor(string.IsNullOrWhiteSpace(i.Label) ? p.Name : i.Label, i.ImageUrl)))
            .ToList();

        var sizes = sizeProducts
            .Select(p => new PartyBoxSize(p.Id, p.Name, PartyBoxRules.ParsePieceCount(p.ServingInfo), p.Price))
            .ToList();

        return new PartyBoxOptions(sizes, flavors);
    }
}
