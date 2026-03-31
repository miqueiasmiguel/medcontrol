using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Users.Queries.GetCurrentUser;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Users;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _sut = new GetCurrentUserQueryHandler(_currentUser, _userRepository, _tenantRepository);
    }

    [Fact]
    public async Task Handle_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_DeveRetornarNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_UsuarioEncontrado_DeveRetornarUserDto()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("user@example.com", "João Silva").Value;
        _currentUser.UserId.Returns(userId);
        _currentUser.TenantId.Returns((Guid?)null);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@example.com");
        result.Value.DisplayName.Should().Be("João Silva");
        result.Value.TenantName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UsuarioComTenant_DeveRetornarTenantName()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = User.Create("user@example.com", "João Silva").Value;
        var tenant = Tenant.Create("Clínica Exemplo").Value;
        _currentUser.UserId.Returns(userId);
        _currentUser.TenantId.Returns((Guid?)tenantId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantName.Should().Be("Clínica Exemplo");
    }
}
