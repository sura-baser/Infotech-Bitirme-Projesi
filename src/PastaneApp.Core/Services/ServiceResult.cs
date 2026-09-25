namespace PastaneApp.Core.Services;

public enum ServiceErrorKind
{
    None,
    NotFound,
    Invalid
}

public record ServiceResult(bool Success, ServiceErrorKind Kind = ServiceErrorKind.None, string? Message = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult NotFound() => new(false, ServiceErrorKind.NotFound);
    public static ServiceResult Invalid(string message) => new(false, ServiceErrorKind.Invalid, message);
}
