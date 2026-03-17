using System.Net;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;

namespace MedControl.Api.Tests.Auth;

public sealed class LogoutEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogoutEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_auth_logout_DeveExpirarOsTresCookies()
    {
        var response = await _client.PostAsync("/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("mmc_access_token=") && c.Contains("max-age=0"));
        cookies.Should().Contain(c => c.Contains("mmc_refresh_token=") && c.Contains("max-age=0"));
        cookies.Should().Contain(c => c.Contains("mmc_session=") && c.Contains("max-age=0"));
    }
}
