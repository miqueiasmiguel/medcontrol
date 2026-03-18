using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Doctors.DTOs;
using MedControl.Domain.Doctors;
using NSubstitute;

namespace MedControl.Api.Tests.Doctors;

public sealed class DoctorEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DoctorEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // GET /doctors

    [Fact]
    public async Task GET_doctors_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/doctors");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_doctors_Autenticado_ComTenant_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.DoctorRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DoctorProfile>());

        var response = await client.GetAsync("/doctors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<DoctorDto>>();
        body.Should().BeEmpty();
    }

    // POST /doctors

    [Fact]
    public async Task POST_doctors_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PostAsJsonAsync("/doctors", new
        {
            name = "Dr. João Silva",
            crm = "123456",
            councilState = "SP",
            specialty = "Cardiologia",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_doctors_CamposInvalidos_Retorna400()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var response = await client.PostAsJsonAsync("/doctors", new
        {
            name = string.Empty,
            crm = string.Empty,
            councilState = string.Empty,
            specialty = string.Empty,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_doctors_CRMDuplicado_Retorna409()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.DoctorRepository
            .ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await client.PostAsJsonAsync("/doctors", new
        {
            name = "Dr. João Silva",
            crm = "123456",
            councilState = "SP",
            specialty = "Cardiologia",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_doctors_DadosValidos_Retorna201ComDoctorDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.DoctorRepository
            .ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await client.PostAsJsonAsync("/doctors", new
        {
            name = "Dr. João Silva",
            crm = "123456",
            councilState = "SP",
            specialty = "Cardiologia",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<DoctorDto>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Dr. João Silva");
        body.Crm.Should().Be("123456");
        body.CouncilState.Should().Be("SP");
        body.Specialty.Should().Be("Cardiologia");
    }
}
