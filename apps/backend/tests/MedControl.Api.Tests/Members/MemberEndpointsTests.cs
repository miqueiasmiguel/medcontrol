using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Members.DTOs;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Api.Tests.Members;

public sealed class MemberEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MemberEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateUnauthenticatedClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

    // GET /members

    [Fact]
    public async Task GET_members_SemAutenticacao_Retorna401()
    {
        var response = await CreateUnauthenticatedClient().GetAsync("/members");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_members_Autenticado_ComTenant_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _factory.UserRepository
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<User>().AsReadOnly() as IReadOnlyList<User>);

        var response = await client.GetAsync("/members");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<MemberDto>>();
        body.Should().BeEmpty();
    }

    // POST /members

    [Fact]
    public async Task POST_members_SemAutenticacao_Retorna401()
    {
        var response = await CreateUnauthenticatedClient().PostAsJsonAsync("/members", new
        {
            email = "new@example.com",
            role = "operator",
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_members_UsuarioSemPermissaoDeAdmin_Retorna401()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId, roles: ["operator"]);

        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(Tenant.Create("Clinic").Value);

        var response = await client.PostAsJsonAsync("/members", new
        {
            email = "new@example.com",
            role = "operator",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_members_CamposInvalidos_Retorna400()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId, roles: ["admin"]);

        var response = await client.PostAsJsonAsync("/members", new
        {
            email = "not-an-email",
            role = string.Empty,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_members_UsuarioNaoExiste_Retorna201ComInvitedTrue()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        var client = _factory.CreateAuthenticatedClient(userId, "admin@example.com", tenantId, roles: ["admin"]);

        _factory.UserRepository
            .GetByEmailAsync("new-invited@example.com", Arg.Any<CancellationToken>())
            .ReturnsNull();
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _factory.MagicLinkService
            .GenerateTokenAsync("new-invited@example.com", Arg.Any<CancellationToken>())
            .Returns("test-invitation-token");

        var response = await client.PostAsJsonAsync("/members", new
        {
            email = "new-invited@example.com",
            role = "operator",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MemberDto>();
        body.Should().NotBeNull();
        body!.Invited.Should().BeTrue();
        body.Email.Should().Be("new-invited@example.com");
        await _factory.EmailService.Received(1).SendInvitationAsync(
            "new-invited@example.com",
            Arg.Is<string>(url => url.Contains("test-invitation-token")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task POST_members_UsuarioJaMembro_Retorna409()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = User.Create("existing@example.com").Value;
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(user.Id, TenantRole.Operator);

        var client = _factory.CreateAuthenticatedClient(userId, "admin@example.com", tenantId, roles: ["admin"]);

        _factory.UserRepository
            .GetByEmailAsync("existing@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.PostAsJsonAsync("/members", new
        {
            email = "existing@example.com",
            role = "operator",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_members_DadosValidos_Retorna201ComMemberDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var newUser = User.Create("new@example.com", "New User").Value;
        var tenant = Tenant.Create("Clinic").Value;

        var client = _factory.CreateAuthenticatedClient(userId, "admin@example.com", tenantId, roles: ["admin"]);

        _factory.UserRepository
            .GetByEmailAsync("new@example.com", Arg.Any<CancellationToken>())
            .Returns(newUser);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.PostAsJsonAsync("/members", new
        {
            email = "new@example.com",
            role = "operator",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MemberDto>();
        body.Should().NotBeNull();
        body!.Role.Should().Be("operator");
        body.Email.Should().Be("new@example.com");
        body.Invited.Should().BeFalse();
    }

    // PATCH /members/{userId}

    [Fact]
    public async Task PATCH_members_userId_SemAutenticacao_Retorna401()
    {
        var response = await CreateUnauthenticatedClient().PatchAsJsonAsync($"/members/{Guid.NewGuid()}", new
        {
            role = "doctor",
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PATCH_members_userId_MembroNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;

        var client = _factory.CreateAuthenticatedClient(userId, "admin@example.com", tenantId, roles: ["admin"]);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.PatchAsJsonAsync($"/members/{targetUserId}", new
        {
            role = "doctor",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_members_userId_DadosValidos_Retorna200ComMemberDto()
    {
        var adminId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(memberId, TenantRole.Operator);

        var memberUser = User.Create("member@example.com", "Member").Value;

        var client = _factory.CreateAuthenticatedClient(adminId, "admin@example.com", tenantId, roles: ["admin"]);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _factory.UserRepository
            .GetByIdAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(memberUser);

        var response = await client.PatchAsJsonAsync($"/members/{memberId}", new
        {
            role = "doctor",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MemberDto>();
        body.Should().NotBeNull();
        body!.Role.Should().Be("doctor");
    }

    // DELETE /members/{userId}

    [Fact]
    public async Task DELETE_members_userId_SemAutenticacao_Retorna401()
    {
        var response = await CreateUnauthenticatedClient().DeleteAsync($"/members/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DELETE_members_userId_MembroNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;

        var client = _factory.CreateAuthenticatedClient(userId, "admin@example.com", tenantId, roles: ["admin"]);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.DeleteAsync($"/members/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_members_userId_MembroOwner_Retorna400()
    {
        var adminId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(ownerId, TenantRole.Owner);

        var client = _factory.CreateAuthenticatedClient(adminId, "admin@example.com", tenantId, roles: ["admin"]);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.DeleteAsync($"/members/{ownerId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_members_userId_DadosValidos_Retorna204()
    {
        var adminId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(memberId, TenantRole.Doctor);

        var client = _factory.CreateAuthenticatedClient(adminId, "admin@example.com", tenantId, roles: ["admin"]);
        _factory.TenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        var response = await client.DeleteAsync($"/members/{memberId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
