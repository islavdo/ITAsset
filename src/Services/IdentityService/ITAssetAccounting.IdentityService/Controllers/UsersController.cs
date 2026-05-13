using ITAssetAccounting.IdentityService.Data;
using ITAssetAccounting.IdentityService.Entities;
using ITAssetAccounting.IdentityService.Services;
using ITAssetAccounting.Shared.Api;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IdentityDbContext _db;
    private readonly AuthService _auth;

    public UsersController(IdentityDbContext db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetAll(CancellationToken ct)
    {
        var users = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
        return Ok(users.Select(AuthService.ToDto).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        return user is null ? NotFound(new ApiError("not_found", "User not found.")) : Ok(AuthService.ToDto(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _auth.CreateUserAsync(request.FullName, request.Email, request.Password, request.Department, request.Roles, ct);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("email_exists", ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UserUpdateRequest request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return NotFound(new ApiError("not_found", "User not found."));
        }

        user.FullName = request.FullName.Trim();
        user.Department = request.Department.Trim();
        user.IsActive = request.IsActive;
        user.UserRoles.Clear();
        foreach (var role in request.Roles.Distinct())
        {
            user.UserRoles.Add(new AppUserRole { UserId = id, RoleId = (int)role });
        }
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object?[] { id }, ct);
        if (user is null)
        {
            return NotFound(new ApiError("not_found", "User not found."));
        }
        user.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
