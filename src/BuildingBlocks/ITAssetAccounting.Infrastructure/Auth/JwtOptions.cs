namespace ITAssetAccounting.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "ITAssetAccounting.IdentityService";
    public string Audience { get; set; } = "ITAssetAccounting.Clients";
    public string SigningKey { get; set; } = "CHANGE_ME_TO_A_LONG_SECRET_KEY_FOR_DEVELOPMENT_ONLY";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}
