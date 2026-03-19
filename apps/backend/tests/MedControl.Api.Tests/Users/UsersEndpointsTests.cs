using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Users.DTOs;
using MedControl.Domain.Users;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Api.Tests.Users;

public sealed class UsersEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public UsersEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // GET /users/me

    [Fact]
    public async Task GET_users_me_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_users_me_UsuarioNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        _factory.UserRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var response = await client.GetAsync("/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_users_me_Autenticado_Retorna200ComUserDto()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("user@example.com", "João Silva").Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        _factory.UserRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var response = await client.GetAsync("/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("user@example.com");
        body.DisplayName.Should().Be("João Silva");
    }

    // PATCH /users/me/profile

    [Fact]
    public async Task PATCH_users_me_profile_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PatchAsJsonAsync("/users/me/profile", new { displayName = "Novo Nome" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PATCH_users_me_profile_UsuarioNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        _factory.UserRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var response = await client.PatchAsJsonAsync("/users/me/profile", new { displayName = "Novo Nome" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_users_me_profile_DisplayNameValido_Retorna200ComUserDto()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("user@example.com", "Nome Antigo").Value;
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        _factory.UserRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var response = await client.PatchAsJsonAsync("/users/me/profile", new { displayName = "Nome Novo" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body.Should().NotBeNull();
        body!.DisplayName.Should().Be("Nome Novo");
    }

    [Fact]
    public async Task PATCH_users_me_profile_DisplayNameMuitoLongo_Retorna400()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com");

        var response = await client.PatchAsJsonAsync("/users/me/profile", new { displayName = new string('A', 101) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
