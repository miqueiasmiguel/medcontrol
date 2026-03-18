using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Tenants.Commands.SwitchTenant;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using NSubstitute;

namespace MedControl.Application.Tests.Tenants;

public sealed class SwitchTenantCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly SwitchTenantCommandHandler _sut;

    public SwitchTenantCommandHandlerTests()
    {
        _sut = new SwitchTenantCommandHandler(_currentUser, _tenantRepository, _tokenService);
    }

    [Fact]
    public async Task Handle_WithValidMembership_ReturnsNewToken()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var tokenPair = new TokenPair("new-access", "new-refresh", DateTimeOffset.UtcNow.AddHours(1));

        var tenant = Tenant.Create("Clínica").Value;
        tenant.AddMember(userId, TenantRole.Operator);

        _currentUser.UserId.Returns(userId);
        _currentUser.Email.Returns(email);
        _currentUser.GlobalRoles.Returns(new List<string>());
        _tenantRepository.ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });
        _tokenService.GenerateTokenPair(
                userId, email, tenant.Id,
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(new SwitchTenantCommand(tenant.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-access");
    }

    [Fact]
    public async Task Handle_WithTenantNotInUserList_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _currentUser.Email.Returns("user@example.com");
        _tenantRepository.ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var result = await _sut.Handle(new SwitchTenantCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithNullUserId_ReturnsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new SwitchTenantCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
