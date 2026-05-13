using ITAssetAccounting.IdentityService.Services;
using ITAssetAccounting.Shared.Api;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccounting.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        try
        {
            await _auth.CreateUserAsync(request.FullName, request.Email, request.Password, request.Department, new[] { request.Role }, ct);
            var token = await _auth.LoginAsync(request.Email, request.Password, ct);
            return Ok(token);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("email_exists", ex.Message));
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var token = await _auth.LoginAsync(request.Email, request.Password, ct);
        return token is null
            ? Unauthorized(new ApiError("invalid_credentials", "Incorrect email or password."))
            : Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var token = await _auth.RefreshAsync(request.RefreshToken, ct);
        return token is null
            ? Unauthorized(new ApiError("invalid_refresh_token", "Refresh token is invalid or expired."))
            : Ok(token);
    }
}
