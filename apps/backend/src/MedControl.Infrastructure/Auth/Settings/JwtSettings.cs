namespace MedControl.Infrastructure.Auth.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenExpiryMinutes { get; init; } = 60;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}
