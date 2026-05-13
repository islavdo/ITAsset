using ITAssetAccounting.IdentityService.Entities;
using ITAssetAccounting.IdentityService.Services;
using ITAssetAccounting.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.IdentityService.Data;

public static class IdentitySeedData
{
    public static async Task SeedAsync(IdentityDbContext db, AuthService authService)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        await authService.CreateUserAsync("System Administrator", "admin@it.local", "Admin123!", "IT", new[] { UserRole.Admin, UserRole.ItSpecialist, UserRole.Manager });
        await authService.CreateUserAsync("IT Specialist", "it@it.local", "Admin123!", "IT", new[] { UserRole.ItSpecialist });
        await authService.CreateUserAsync("Employee Demo", "employee@it.local", "Admin123!", "Accounting", new[] { UserRole.Employee });
    }
}
