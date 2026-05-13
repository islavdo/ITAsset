using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.Shared.Dto;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    string Department);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    UserDto User);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Department,
    IReadOnlyCollection<UserRole> Roles,
    DateTime CreatedAtUtc,
    bool IsActive);

public record UserCreateRequest(
    string FullName,
    string Email,
    string Password,
    string Department,
    IReadOnlyCollection<UserRole> Roles);

public record UserUpdateRequest(
    string FullName,
    string Department,
    bool IsActive,
    IReadOnlyCollection<UserRole> Roles);
