namespace PastaneApp.Core.Services;

public record PartyBoxSize(int ProductId, string Name, int PieceCount, decimal Price);

public record PartyBoxFlavor(string Label, string? ImageUrl);

public record PartyBoxOptions(IReadOnlyList<PartyBoxSize> Sizes, IReadOnlyList<PartyBoxFlavor> Flavors);

public interface IPartyBoxService
{
    Task<PartyBoxOptions> GetOptionsAsync();
}
