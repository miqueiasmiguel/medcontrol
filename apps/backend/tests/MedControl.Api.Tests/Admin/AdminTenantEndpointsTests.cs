using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Admin.DTOs;
using MedControl.Domain.Tenants;
using NSubstitute;

namespace MedControl.Api.Tests.Admin;

public sealed class AdminTenantEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly Guid _adminUserId = Guid.NewGuid();
    private const string AdminEmail = "admin@example.com";

    public AdminTenantEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── GET /admin/tenants ──────────────────────────────────────────────────

    [Fact]
    public async Task GetTenants_NoAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/admin/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTenants_AuthenticatedWithoutGlobalAdmin_Returns403()
    {
        var client = _factory.CreateAuthenticatedClient(_adminUserId, AdminEmail);
        _factory.TenantRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var response = await client.GetAsync("/admin/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTenants_GlobalAdmin_Returns200WithList()
    {
        var client = _factory.CreateAuthenticatedClient(
            _adminUserId, AdminEmail, globalRoles: ["admin"]);

        var tenant = Tenant.Create("Clinic A").Value;
        _factory.TenantRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });

        var response = await client.GetAsync("/admin/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtos = await response.Content.ReadFromJsonAsync<AdminTenantDto[]>();
        dtos.Should().HaveCount(1);
        dtos![0].Name.Should().Be("Clinic A");
    }

    // ── PATCH /admin/tenants/{id}/status ───────────────────────────────────

    [Fact]
    public async Task PatchStatus_NoAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/admin/tenants/{Guid.NewGuid()}/status", new { isActive = false });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatchStatus_AuthenticatedWithoutGlobalAdmin_Returns403()
    {
        var client = _factory.CreateAuthenticatedClient(_adminUserId, AdminEmail);
        var tenant = Tenant.Create("Clinic").Value;
        _factory.TenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.PatchAsJsonAsync(
            $"/admin/tenants/{tenant.Id}/status", new { isActive = false });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PatchStatus_TenantNotFound_Returns404()
    {
        var client = _factory.CreateAuthenticatedClient(
            _adminUserId, AdminEmail, globalRoles: ["admin"]);
        _factory.TenantRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Tenant?)null);

        var response = await client.PatchAsJsonAsync(
            $"/admin/tenants/{Guid.NewGuid()}/status", new { isActive = false });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PatchStatus_GlobalAdmin_Deactivate_Returns204()
    {
        var client = _factory.CreateAuthenticatedClient(
            _adminUserId, AdminEmail, globalRoles: ["admin"]);
        var tenant = Tenant.Create("Active Clinic").Value;
        _factory.TenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.PatchAsJsonAsync(
            $"/admin/tenants/{tenant.Id}/status", new { isActive = false });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task PatchStatus_GlobalAdmin_Activate_Returns204()
    {
        var client = _factory.CreateAuthenticatedClient(
            _adminUserId, AdminEmail, globalRoles: ["admin"]);
        var tenant = Tenant.Create("Inactive Clinic").Value;
        tenant.Deactivate();
        _factory.TenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.PatchAsJsonAsync(
            $"/admin/tenants/{tenant.Id}/status", new { isActive = true });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        tenant.IsActive.Should().BeTrue();
    }
}
