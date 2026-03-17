using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MedControl.Application.Common.Interfaces;
using MedControl.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MedControl.Infrastructure.Auth;

internal sealed class TokenService(
    IDistributedCache cache,
    IOptions<JwtSettings> settings)
    : ITokenService
{
    private static string RefreshKey(string token) => $"refresh_token:{token}";

    public TokenPair GenerateTokenPair(
        Guid userId,
        string email,
        Guid? tenantId,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> globalRoles)
    {
        var jwt = settings.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(jwt.AccessTokenExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        foreach (var globalRole in globalRoles)
        {
            claims.Add(new Claim("global_roles", globalRole));
        }

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiry.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        var refreshToken = GenerateRefreshToken();

        _ = StoreRefreshTokenAsync(refreshToken, userId, jwt.RefreshTokenExpiryDays);

        return new TokenPair(accessToken, refreshToken, expiry);
    }

    public async Task<string?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await cache.GetStringAsync(RefreshKey(refreshToken), ct);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        await cache.RemoveAsync(RefreshKey(refreshToken), ct);
    }

    private async Task StoreRefreshTokenAsync(string token, Guid userId, int expiryDays)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(expiryDays),
        };
        await cache.SetStringAsync(RefreshKey(token), userId.ToString(), options);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
