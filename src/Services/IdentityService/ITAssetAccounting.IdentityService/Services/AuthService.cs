using ITAssetAccounting.IdentityService.Data;
using ITAssetAccounting.IdentityService.Entities;
using ITAssetAccounting.Infrastructure.Auth;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ITAssetAccounting.IdentityService.Services;

public class AuthService
{
    private readonly IdentityDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly PasswordHasher<AppUser> _hasher = new();
    private readonly JwtOptions _options;

    public AuthService(IdentityDbContext db, JwtTokenService jwt, IOptions<JwtOptions> options)
    {
        _db = db;
        _jwt = jwt;
        _options = options.Value;
    }

    public async Task<UserDto> CreateUserAsync(string fullName, string email, string password, string department, IEnumerable<UserRole> roles, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
        {
            throw new InvalidOperationException("User with the same email already exists.");
        }

        var user = new AppUser
        {
            FullName = fullName.Trim(),
            Email = email,
            Department = department.Trim(),
            IsActive = true
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        foreach (var role in roles.Distinct())
        {
            user.UserRoles.Add(new AppUserRole { UserId = user.Id, RoleId = (int)role });
        }

        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<TokenResponse?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);
        if (user is null)
        {
            return null;
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var (access, expires) = _jwt.CreateAccessToken(user);
        var refresh = _jwt.CreateRefreshToken();
        user.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(refresh),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
        });
        await _db.SaveChangesAsync(ct);

        return new TokenResponse(access, refresh, expires, ToDto(user));
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = JwtTokenService.HashRefreshToken(refreshToken);
        var token = await _db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive || !token.User.IsActive)
        {
            return null;
        }

        token.RevokedAtUtc = DateTime.UtcNow;
        var newRefresh = _jwt.CreateRefreshToken();
        token.User.RefreshTokens.Add(new RefreshToken
        {
            UserId = token.UserId,
            TokenHash = JwtTokenService.HashRefreshToken(newRefresh),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
        });

        var (access, expires) = _jwt.CreateAccessToken(token.User);
        await _db.SaveChangesAsync(ct);
        return new TokenResponse(access, newRefresh, expires, ToDto(token.User));
    }

    public static UserDto ToDto(AppUser user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.Department,
        user.UserRoles.Select(ur => ur.Role.Name).ToArray(),
        user.CreatedAtUtc,
        user.IsActive);
}
