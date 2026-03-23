namespace MedControl.Application.Common.Interfaces;

public record GoogleUserInfo(string Email, string DisplayName, Uri? AvatarUrl);

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ExchangeCodeAsync(string code, string redirectUri, CancellationToken ct = default);
    Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}
