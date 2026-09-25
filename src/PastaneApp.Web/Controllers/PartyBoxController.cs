using Microsoft.AspNetCore.Mvc;
using PastaneApp.Core.Services;
using PastaneApp.Web.Models.PartyBox;

namespace PastaneApp.Web.Controllers;

public class PartyBoxController : Controller
{
    private readonly IPartyBoxService _partyBoxService;

    public PartyBoxController(IPartyBoxService partyBoxService)
    {
        _partyBoxService = partyBoxService;
    }

    public async Task<IActionResult> Index()
    {
        var options = await _partyBoxService.GetOptionsAsync();

        var model = new PartyBoxIndexViewModel
        {
            Sizes = options.Sizes.Select(s => new PartyBoxSizeOption
            {
                ProductId = s.ProductId,
                Name = s.Name,
                PieceCount = s.PieceCount,
                Price = s.Price
            }).ToList(),
            Flavors = options.Flavors.Select(f => new PartyBoxFlavorOption
            {
                Label = f.Label,
                ImageUrl = f.ImageUrl
            }).ToList()
        };

        return View(model);
    }
}
