using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Tenants.Queries.GetMyTenants;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using NSubstitute;

namespace MedControl.Application.Tests.Tenants;

public sealed class GetMyTenantsQueryHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly GetMyTenantsQueryHandler _sut;

    public GetMyTenantsQueryHandlerTests()
    {
        _sut = new GetMyTenantsQueryHandler(_currentUser, _tenantRepository);
    }

    [Fact]
    public async Task Handle_WithNoTenants_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _tenantRepository.ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var result = await _sut.Handle(new GetMyTenantsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithTenants_ReturnsTenantDtos()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        var tenant = Tenant.Create("Clínica Saúde").Value;
        tenant.AddMember(userId, TenantRole.Admin);
        _tenantRepository.ListByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });

        var result = await _sut.Handle(new GetMyTenantsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Clínica Saúde");
        result.Value[0].Role.Should().Be("admin");
    }

    [Fact]
    public async Task Handle_WithNullUserId_ReturnsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetMyTenantsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
