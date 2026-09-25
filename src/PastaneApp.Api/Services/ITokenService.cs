using PastaneApp.Core.Entities;

namespace PastaneApp.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
