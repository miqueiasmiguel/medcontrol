using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;

namespace MedControl.Api.Tests.Health;

public sealed class HealthEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task GET_health_DeveRetornar200_QuandoApiEBancoSaudaveis()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_health_DeveRetornarBodyComStatusSaudavelEChecks()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().Should().Be("healthy");
        body.GetProperty("checks").GetProperty("database").GetString().Should().Be("healthy");
    }

    [Fact]
    public async Task GET_health_NaoDeveRequerirAutenticacao()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
