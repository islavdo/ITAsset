using ITAssetAccounting.IdentityService.Entities;
using ITAssetAccounting.IdentityService.Services;
using ITAssetAccounting.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.IdentityService.Data;

public static class IdentitySeedData
{
    public static async Task SeedAsync(IdentityDbContext db, AuthService authService)
    {
        await CreateIfMissingAsync(db, authService, "System Administrator", "admin@it.local", "Admin123!", "IT", new[] { UserRole.Admin, UserRole.ItSpecialist, UserRole.Manager });
        await CreateIfMissingAsync(db, authService, "IT Specialist", "it@it.local", "Admin123!", "IT", new[] { UserRole.ItSpecialist });
        await CreateIfMissingAsync(db, authService, "Employee Demo", "employee@it.local", "Admin123!", "Accounting", new[] { UserRole.Employee });
    }

    private static async Task CreateIfMissingAsync(
        IdentityDbContext db,
        AuthService authService,
        string fullName,
        string email,
        string password,
        string department,
        IEnumerable<UserRole> roles)
    {
        if (!await db.Users.AnyAsync(u => u.Email == email))
        {
            await authService.CreateUserAsync(fullName, email, password, department, roles);
        }
    }
}
