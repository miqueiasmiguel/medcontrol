using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Users;
using NSubstitute;

namespace MedControl.Api.Tests.Auth;

public sealed class MagicLinkEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MagicLinkEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_auth_magic_link_send_ValidEmail_Returns204()
    {
        _factory.UserRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(User.Create("user@example.com").Value);
        _factory.MagicLinkService.GenerateTokenAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns("token123");

        var response = await _client.PostAsJsonAsync("/auth/magic-link/send",
            new { email = "user@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_auth_magic_link_send_InvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/magic-link/send",
            new { email = "not-an-email" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_auth_magic_link_verify_ValidToken_Returns200WithTokens()
    {
        var email = "user@example.com";
        var user = User.Create(email).Value;
        var tokenPair = new TokenPair("access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

        _factory.MagicLinkService.ValidateTokenAsync("valid-token", Arg.Any<CancellationToken>())
            .Returns(email);
        _factory.UserRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _factory.TokenService.GenerateTokenPair(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var response = await _client.PostAsJsonAsync("/auth/magic-link/verify",
            new { token = "valid-token" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<AuthTokenDto>();
        dto!.AccessToken.Should().Be("access-token");
        dto.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task POST_auth_magic_link_verify_InvalidToken_Returns401()
    {
        _factory.MagicLinkService.ValidateTokenAsync("expired", Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var response = await _client.PostAsJsonAsync("/auth/magic-link/verify",
            new { token = "expired" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
