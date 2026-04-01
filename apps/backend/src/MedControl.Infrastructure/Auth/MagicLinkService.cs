using System.Security.Cryptography;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace MedControl.Infrastructure.Auth;

internal sealed class MagicLinkService(
    IDistributedCache cache,
    IOptions<MagicLinkSettings> settings)
    : IMagicLinkService
{
    private static string CacheKey(string token) => $"magic_link:{token}";

    public async Task<string> GenerateTokenAsync(string email, CancellationToken ct = default)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64UrlEncode(bytes);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.Value.TokenExpiryMinutes),
        };

        await cache.SetStringAsync(CacheKey(token), email, options, ct);
        return token;
    }

    public async Task<string> GenerateInviteTokenAsync(string email, CancellationToken ct = default)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64UrlEncode(bytes);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(settings.Value.InviteTokenExpiryHours),
        };

        await cache.SetStringAsync(CacheKey(token), email, options, ct);
        return token;
    }

    public async Task<string?> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var key = CacheKey(token);
        var email = await cache.GetStringAsync(key, ct);
        if (email is null)
        {
            return null;
        }

        await cache.RemoveAsync(key, ct);
        return email;
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
