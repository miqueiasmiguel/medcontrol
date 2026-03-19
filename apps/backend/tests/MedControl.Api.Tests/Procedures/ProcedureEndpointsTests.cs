using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Procedures;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Api.Tests.Procedures;

public sealed class ProcedureEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public ProcedureEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // GET /procedures

    [Fact]
    public async Task GET_procedures_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/procedures");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_procedures_Autenticado_ComTenant_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .ListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new List<Procedure>());

        var response = await client.GetAsync("/procedures");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ProcedureDto>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_procedures_ComActiveOnlyFalse_Retorna200()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .ListAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<Procedure>());

        var response = await client.GetAsync("/procedures?activeOnly=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // POST /procedures

    [Fact]
    public async Task POST_procedures_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PostAsJsonAsync("/procedures", new
        {
            code = "10101012",
            description = "Consulta médica",
            value = 150.00,
            effectiveFrom = Today,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_procedures_CamposInvalidos_Retorna400()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var response = await client.PostAsJsonAsync("/procedures", new
        {
            code = string.Empty,
            description = string.Empty,
            value = 0,
            effectiveFrom = Today,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_procedures_CodeDuplicado_Retorna409()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .ExistsByCodeAndEffectiveFromAsync(tenantId, "10101012", Today, Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await client.PostAsJsonAsync("/procedures", new
        {
            code = "10101012",
            description = "Consulta médica",
            value = 150.00,
            effectiveFrom = Today,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_procedures_DadosValidos_Retorna201ComProcedureDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .ExistsByCodeAndEffectiveFromAsync(tenantId, "10101012", Today, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await client.PostAsJsonAsync("/procedures", new
        {
            code = "10101012",
            description = "Consulta médica",
            value = 150.00,
            effectiveFrom = Today,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProcedureDto>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("10101012");
        body.Description.Should().Be("Consulta médica");
        body.Value.Should().Be(150.00m);
        body.EffectiveFrom.Should().Be(Today);
    }

    // PATCH /procedures/{id}

    [Fact]
    public async Task PATCH_procedures_id_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PatchAsJsonAsync($"/procedures/{Guid.NewGuid()}", new
        {
            code = "20202025",
            description = "Consulta especializada",
            value = 300.00,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PATCH_procedures_id_ProcedimentoNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var procedureId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .GetByIdAsync(procedureId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var response = await client.PatchAsJsonAsync($"/procedures/{procedureId}", new
        {
            code = "20202025",
            description = "Consulta especializada",
            value = 300.00,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_procedures_id_CodeDuplicado_Retorna409()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var procedure = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, Today).Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .GetByIdAsync(procedure.Id, Arg.Any<CancellationToken>())
            .Returns(procedure);
        _factory.ProcedureRepository
            .ExistsByCodeAsync(tenantId, "99999999", Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await client.PatchAsJsonAsync($"/procedures/{procedure.Id}", new
        {
            code = "99999999",
            description = "Consulta médica",
            value = 150.00,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PATCH_procedures_id_DadosValidos_Retorna200ComProcedureDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var procedure = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, Today).Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureRepository
            .GetByIdAsync(procedure.Id, Arg.Any<CancellationToken>())
            .Returns(procedure);
        _factory.ProcedureRepository
            .ExistsByCodeAsync(tenantId, "20202025", Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await client.PatchAsJsonAsync($"/procedures/{procedure.Id}", new
        {
            code = "20202025",
            description = "Consulta especializada",
            value = 300.00,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProcedureDto>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("20202025");
        body.Description.Should().Be("Consulta especializada");
        body.Value.Should().Be(300.00m);
    }

    // GET /procedures/imports

    [Fact]
    public async Task GET_procedures_imports_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/procedures/imports");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_procedures_imports_Autenticado_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.ProcedureImportRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ProcedureImport>());

        var response = await client.GetAsync("/procedures/imports");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ProcedureImportDto>>();
        body.Should().BeEmpty();
    }
}
