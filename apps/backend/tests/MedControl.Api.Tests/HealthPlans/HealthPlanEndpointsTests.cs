using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.HealthPlans.DTOs;
using MedControl.Domain.HealthPlans;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Api.Tests.HealthPlans;

public sealed class HealthPlanEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public HealthPlanEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // GET /health-plans

    [Fact]
    public async Task GET_health_plans_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/health-plans");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_health_plans_Autenticado_ComTenant_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.HealthPlanRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<HealthPlan>());

        var response = await client.GetAsync("/health-plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<HealthPlanDto>>();
        body.Should().BeEmpty();
    }

    // POST /health-plans

    [Fact]
    public async Task POST_health_plans_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PostAsJsonAsync("/health-plans", new
        {
            name = "Unimed",
            tissCode = "11111119",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_health_plans_CamposInvalidos_Retorna400()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var response = await client.PostAsJsonAsync("/health-plans", new
        {
            name = string.Empty,
            tissCode = string.Empty,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_health_plans_TissCodeDuplicado_Retorna409()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.HealthPlanRepository
            .ExistsByTissCodeAsync(tenantId, "11111119", Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await client.PostAsJsonAsync("/health-plans", new
        {
            name = "Unimed",
            tissCode = "11111119",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_health_plans_DadosValidos_Retorna201ComHealthPlanDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.HealthPlanRepository
            .ExistsByTissCodeAsync(tenantId, "11111119", Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await client.PostAsJsonAsync("/health-plans", new
        {
            name = "Unimed",
            tissCode = "11111119",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<HealthPlanDto>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Unimed");
        body.TissCode.Should().Be("11111119");
    }

    // PATCH /health-plans/{id}

    [Fact]
    public async Task PATCH_health_plans_id_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PatchAsJsonAsync($"/health-plans/{Guid.NewGuid()}", new
        {
            name = "Amil",
            tissCode = "22222224",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PATCH_health_plans_id_ConvenioNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var healthPlanId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.HealthPlanRepository
            .GetByIdAsync(healthPlanId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var response = await client.PatchAsJsonAsync($"/health-plans/{healthPlanId}", new
        {
            name = "Amil",
            tissCode = "22222224",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_health_plans_id_TissCodeDuplicado_Retorna409()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var healthPlan = HealthPlan.Create(tenantId, "Unimed", "11111119").Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.HealthPlanRepository
            .GetByIdAsync(healthPlan.Id, Arg.Any<CancellationToken>())
            .Returns(healthPlan);
        _factory.HealthPlanRepository
            .ExistsByTissCodeAsync(tenantId, "99999999", Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await client.PatchAsJsonAsync($"/health-plans/{healthPlan.Id}", new
        {
            name = "Unimed",
            tissCode = "99999999",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PATCH_health_plans_id_DadosValidos_Retorna200ComHealthPlanDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var healthPlan = HealthPlan.Create(tenantId, "Unimed", "11111119").Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.HealthPlanRepository
            .GetByIdAsync(healthPlan.Id, Arg.Any<CancellationToken>())
            .Returns(healthPlan);
        _factory.HealthPlanRepository
            .ExistsByTissCodeAsync(tenantId, "22222224", Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await client.PatchAsJsonAsync($"/health-plans/{healthPlan.Id}", new
        {
            name = "Amil",
            tissCode = "22222224",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HealthPlanDto>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Amil");
        body.TissCode.Should().Be("22222224");
    }
}
