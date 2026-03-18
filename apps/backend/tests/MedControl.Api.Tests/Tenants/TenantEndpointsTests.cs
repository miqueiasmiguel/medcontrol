using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Tenants;
using NSubstitute;

namespace MedControl.Api.Tests.Tenants;

public sealed class TenantEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TenantEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // GET /tenants/me
    [Fact]
    public async Task GET_tenants_me_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/tenants/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_tenants_me_Autenticado_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        _factory.TenantRepository
            .ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var response = await client.GetAsync("/tenants/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // POST /tenants
    [Fact]
    public async Task POST_tenants_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });
        var response = await client.PostAsJsonAsync("/tenants", new { name = "Clinic" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_tenants_NomeVazio_Retorna400()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        var response = await client.PostAsJsonAsync("/tenants", new { name = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_tenants_ValoresValidos_Retorna204ComCookies()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var client = _factory.CreateAuthenticatedClient(userId, email);
        var tokenPair = new TokenPair("access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

        _factory.TokenService.GenerateTokenPair(
                userId, email, Arg.Any<Guid>(),
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var response = await client.PostAsJsonAsync("/tenants", new { name = "Minha Clínica" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("mmc_access_token=") && c.Contains("httponly"));
    }

    // POST /tenants/switch
    [Fact]
    public async Task POST_tenants_switch_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });
        var response = await client.PostAsJsonAsync("/tenants/switch", new { tenantId = Guid.NewGuid() });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_tenants_switch_TenantNaoMembro_Retorna404()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        _factory.TenantRepository
            .ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var response = await client.PostAsJsonAsync("/tenants/switch", new { tenantId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_tenants_switch_MembroValido_Retorna204ComCookies()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var client = _factory.CreateAuthenticatedClient(userId, email);
        var tokenPair = new TokenPair("new-access", "new-refresh", DateTimeOffset.UtcNow.AddHours(1));

        var tenant = Tenant.Create("Clínica").Value;
        tenant.AddMember(userId, TenantRole.Operator);

        _factory.TenantRepository
            .ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });
        _factory.TokenService.GenerateTokenPair(
                userId, email, tenant.Id,
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var response = await client.PostAsJsonAsync("/tenants/switch", new { tenantId = tenant.Id });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("mmc_access_token=") && c.Contains("httponly"));
    }
}
