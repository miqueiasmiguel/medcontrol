namespace MedControl.Application.Auth.Settings;

public sealed class MagicLinkSettings
{
    public const string SectionName = "MagicLink";
    public string BaseUrl { get; init; } = default!;
    public int TokenExpiryMinutes { get; init; } = 15;
}
