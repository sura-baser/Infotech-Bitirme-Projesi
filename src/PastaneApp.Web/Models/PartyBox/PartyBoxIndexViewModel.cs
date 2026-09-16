namespace PastaneApp.Web.Models.PartyBox;

public class PartyBoxIndexViewModel
{
    public List<PartyBoxSizeOption> Sizes { get; set; } = new();
    public List<PartyBoxFlavorOption> Flavors { get; set; } = new();
}

public class PartyBoxSizeOption
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PieceCount { get; set; }
    public decimal Price { get; set; }
}

public class PartyBoxFlavorOption
{
    public string Label { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
