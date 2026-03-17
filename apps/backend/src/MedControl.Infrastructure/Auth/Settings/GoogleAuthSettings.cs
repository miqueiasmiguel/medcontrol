namespace MedControl.Infrastructure.Auth.Settings;

public sealed class GoogleAuthSettings
{
    public const string SectionName = "Google";

    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
}
