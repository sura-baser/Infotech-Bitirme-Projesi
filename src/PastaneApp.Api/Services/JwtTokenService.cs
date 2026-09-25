using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PastaneApp.Core.Entities;

namespace PastaneApp.Api.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public const string NameClaimType = "name";
    public const string RoleClaimType = "role";

    public string Issuer { get; set; } = "AtelierSura";
    public string Audience { get; set; } = "AtelierSura.Api";
    public string Key { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
}

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtOptions.NameClaimType, user.FullName ?? user.UserName ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(JwtOptions.RoleClaimType, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
