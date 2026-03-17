using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Users;
using NSubstitute;

namespace MedControl.Api.Tests.Auth;

public sealed class GoogleAuthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GoogleAuthEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_auth_google_callback_ValoresValidos_Retorna200ComToken()
    {
        var email = "user@example.com";
        var user = User.CreateFromGoogle(email, "User Name", null).Value;
        var tokenPair = new TokenPair("access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));
        var googleUserInfo = new GoogleUserInfo(email, "User Name", null);

        _factory.GoogleAuthService
            .ExchangeCodeAsync("valid-code", "http://localhost:4200/auth/callback", Arg.Any<CancellationToken>())
            .Returns(googleUserInfo);
        _factory.UserRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _factory.TokenService.GenerateTokenPair(
                Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var response = await _client.PostAsJsonAsync("/auth/google/callback",
            new { code = "valid-code", redirectUri = "http://localhost:4200/auth/callback" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<AuthTokenDto>();
        dto!.AccessToken.Should().Be("access-token");
        dto.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task POST_auth_google_callback_GoogleFalhou_Retorna401()
    {
        _factory.GoogleAuthService
            .ExchangeCodeAsync("bad-code", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GoogleUserInfo?)null);

        var response = await _client.PostAsJsonAsync("/auth/google/callback",
            new { code = "bad-code", redirectUri = "http://localhost:4200/auth/callback" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
