using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.IdentityService.Entities;

public class AppRole
{
    public int Id { get; set; }
    public UserRole Name { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
