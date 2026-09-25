namespace PastaneApp.Services;

internal static class PartyBoxRules
{
    public static int ParsePieceCount(string? servingInfo)
    {
        if (string.IsNullOrWhiteSpace(servingInfo))
        {
            return 0;
        }

        var digits = new string(servingInfo.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var count) ? count : 0;
    }
}
