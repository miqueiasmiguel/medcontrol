namespace MedControl.Application.Common.Interfaces;

public record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    TokenPair GenerateTokenPair(Guid userId, string email, Guid? tenantId, IReadOnlyList<string> roles, IReadOnlyList<string> globalRoles);
    Task<string?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
