using FluentAssertions;
using MedControl.Application.Auth.Settings;
using MedControl.Infrastructure.Auth;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MedControl.Infrastructure.Tests.Auth;

public sealed class MagicLinkServiceTests
{
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly IOptions<MagicLinkSettings> _settings = Options.Create(new MagicLinkSettings
    {
        TokenExpiryMinutes = 15,
        BaseUrl = "https://app.example.com/auth/verify",
    });

    private readonly MagicLinkService _sut;

    public MagicLinkServiceTests()
    {
        _sut = new MagicLinkService(_cache, _settings);
    }

    [Fact]
    public async Task GenerateTokenAsync_StoresEmailInCache_ReturnsToken()
    {
        var email = "user@example.com";

        var token = await _sut.GenerateTokenAsync(email);

        token.Should().NotBeNullOrEmpty();
        await _cache.Received(1).SetAsync(
            Arg.Is<string>(k => k.StartsWith("magic_link:")),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateTokenAsync_ProducesUniqueTokens()
    {
        var token1 = await _sut.GenerateTokenAsync("a@example.com");
        var token2 = await _sut.GenerateTokenAsync("b@example.com");

        token1.Should().NotBe(token2);
    }

    [Fact]
    public async Task ValidateTokenAsync_CacheMiss_ReturnsNull()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((byte[]?)null);

        var result = await _sut.ValidateTokenAsync("unknown-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_CacheHit_ReturnsEmailAndRemovesKey()
    {
        var email = "user@example.com";
        var token = "valid-token";
        var key = $"magic_link:{token}";

        _cache.GetAsync(key, Arg.Any<CancellationToken>())
              .Returns(System.Text.Encoding.UTF8.GetBytes(email));

        var result = await _sut.ValidateTokenAsync(token);

        result.Should().Be(email);
        await _cache.Received(1).RemoveAsync(key, Arg.Any<CancellationToken>());
    }
}
