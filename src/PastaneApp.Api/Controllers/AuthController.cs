using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Api.Dtos;
using PastaneApp.Api.Services;
using PastaneApp.Core.Entities;

namespace PastaneApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName?.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new ProblemDetails { Title = "E-posta veya şifre hatalı." });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(StatusCodes.Status423Locked, new ProblemDetails { Title = "Hesap geçici olarak kilitlendi, daha sonra tekrar deneyin." });
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new ProblemDetails { Title = "E-posta veya şifre hatalı." });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var (token, expiresAtUtc) = _tokenService.CreateToken(user, roles);

        return new LoginResponse(token, expiresAtUtc, user.Email ?? string.Empty, roles);
    }
}
