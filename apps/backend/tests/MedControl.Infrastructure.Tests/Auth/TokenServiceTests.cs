using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using MedControl.Infrastructure.Auth;
using MedControl.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace MedControl.Infrastructure.Tests.Auth;

public sealed class TokenServiceTests
{
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly IOptions<JwtSettings> _settings = Options.Create(new JwtSettings
    {
        Secret = "super-secret-key-that-is-long-enough-for-hmac-sha256",
        Issuer = "https://medcontrol.app",
        Audience = "medcontrol-api",
        AccessTokenExpiryMinutes = 60,
        RefreshTokenExpiryDays = 30,
    });

    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _sut = new TokenService(_cache, _settings);
    }

    [Fact]
    public void GenerateTokenPair_ReturnsValidJwtAccessToken()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";

        var pair = _sut.GenerateTokenPair(userId, email, null, [], []);

        pair.AccessToken.Should().NotBeNullOrEmpty();
        pair.RefreshToken.Should().NotBeNullOrEmpty();
        pair.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.Secret));
        handler.ValidateToken(pair.AccessToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        }, out var validatedToken);

        validatedToken.Should().NotBeNull();
    }

    [Fact]
    public void GenerateTokenPair_ContainsExpectedClaims()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var tenantId = Guid.NewGuid();

        var pair = _sut.GenerateTokenPair(userId, email, tenantId, ["member"], ["admin"]);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(pair.AccessToken);

        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        jwt.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "roles" && c.Value == "member");
        jwt.Claims.Should().Contain(c => c.Type == "global_roles" && c.Value == "admin");
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsUserIdFromCache()
    {
        var userId = Guid.NewGuid().ToString();
        _cache.GetAsync("refresh_token:mytoken", Arg.Any<CancellationToken>())
              .Returns(Encoding.UTF8.GetBytes(userId));

        var result = await _sut.ValidateRefreshTokenAsync("mytoken");

        result.Should().Be(userId);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_CacheMiss_ReturnsNull()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((byte[]?)null);

        var result = await _sut.ValidateRefreshTokenAsync("unknown");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_RemovesFromCache()
    {
        await _sut.RevokeRefreshTokenAsync("mytoken");

        await _cache.Received(1).RemoveAsync("refresh_token:mytoken", Arg.Any<CancellationToken>());
    }
}
