using System.Net;
using System.Text;
using FluentAssertions;
using MedControl.Infrastructure.Auth;
using MedControl.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Options;

namespace MedControl.Infrastructure.Tests.Auth;

public sealed class GoogleAuthServiceTests
{
    private static GoogleAuthService CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new GoogleAuthSettings
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
        });
        return new GoogleAuthService(httpClient, settings);
    }

    private static FakeHttpHandler OkHandler(string json) =>
        new FakeHttpHandler(HttpStatusCode.OK, json);

    private static FakeHttpHandler ErrorHandler() =>
        new FakeHttpHandler(HttpStatusCode.BadRequest, "{}");

    [Fact]
    public async Task VerifyIdTokenAsync_TokenValido_RetornaGoogleUserInfo()
    {
        const string json = """
            {
                "email": "user@example.com",
                "name": "User Name",
                "picture": "https://example.com/photo.jpg"
            }
            """;
        var sut = CreateSut(OkHandler(json));

        var result = await sut.VerifyIdTokenAsync("valid-id-token");

        result.Should().NotBeNull();
        result!.Email.Should().Be("user@example.com");
        result.DisplayName.Should().Be("User Name");
        result.AvatarUrl.Should().Be(new Uri("https://example.com/photo.jpg"));
    }

    [Fact]
    public async Task VerifyIdTokenAsync_TokenInvalido_RetornaNull()
    {
        var sut = CreateSut(ErrorHandler());

        var result = await sut.VerifyIdTokenAsync("invalid-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task VerifyIdTokenAsync_RespostaSemEmail_RetornaNull()
    {
        const string json = """{ "name": "User Name" }""";
        var sut = CreateSut(OkHandler(json));

        var result = await sut.VerifyIdTokenAsync("token-sem-email");

        result.Should().BeNull();
    }

    [Fact]
    public async Task VerifyIdTokenAsync_SemNome_UsaEmailComoDisplayName()
    {
        const string json = """{ "email": "user@example.com" }""";
        var sut = CreateSut(OkHandler(json));

        var result = await sut.VerifyIdTokenAsync("token-sem-nome");

        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("user@example.com");
    }

    private sealed class FakeHttpHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }
}
