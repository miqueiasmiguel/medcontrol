using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Tenants;
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
    public async Task POST_auth_google_callback_ValoresValidos_Retorna204ComCookies()
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

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("mmc_access_token=") && c.Contains("httponly"));
        cookies.Should().Contain(c => c.StartsWith("mmc_refresh_token=") && c.Contains("httponly"));
        cookies.Should().Contain(c => c.StartsWith("mmc_session=1") && !c.Contains("httponly"));
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

    [Fact]
    public async Task POST_auth_google_verify_IdTokenValido_Retorna204ComCookies()
    {
        var email = "user@example.com";
        var user = User.CreateFromGoogle(email, "User Name", null).Value;
        var tenant = Tenant.Create("Clínica ABC").Value;
        tenant.AddMember(user.Id, TenantRole.Doctor);
        var tokenPair = new TokenPair("access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));
        var googleUserInfo = new GoogleUserInfo(email, "User Name", null);

        _factory.GoogleAuthService
            .VerifyIdTokenAsync("valid-id-token", Arg.Any<CancellationToken>())
            .Returns(googleUserInfo);
        _factory.UserRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _factory.TenantRepository.ListByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });
        _factory.TokenService.GenerateTokenPair(
                Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var response = await _client.PostAsJsonAsync("/auth/google/verify",
            new { idToken = "valid-id-token" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("mmc_access_token=") && c.Contains("httponly"));
        cookies.Should().Contain(c => c.StartsWith("mmc_refresh_token=") && c.Contains("httponly"));
        cookies.Should().Contain(c => c.StartsWith("mmc_session=1") && !c.Contains("httponly"));
    }

    [Fact]
    public async Task POST_auth_google_verify_TokenInvalido_Retorna401()
    {
        _factory.GoogleAuthService
            .VerifyIdTokenAsync("bad-token", Arg.Any<CancellationToken>())
            .Returns((GoogleUserInfo?)null);

        var response = await _client.PostAsJsonAsync("/auth/google/verify",
            new { idToken = "bad-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
