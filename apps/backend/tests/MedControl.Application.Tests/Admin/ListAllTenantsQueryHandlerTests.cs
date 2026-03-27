using FluentAssertions;
using MedControl.Application.Admin.Queries.ListAllTenants;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using NSubstitute;

namespace MedControl.Application.Tests.Admin;

public sealed class ListAllTenantsQueryHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ListAllTenantsQueryHandler _sut;

    public ListAllTenantsQueryHandlerTests()
    {
        _sut = new ListAllTenantsQueryHandler(_currentUser, _tenantRepository);
    }

    [Fact]
    public async Task Handle_NaoGlobalAdmin_RetornaUnauthorized()
    {
        _currentUser.HasGlobalRole("admin").Returns(false);

        var result = await _sut.Handle(new ListAllTenantsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        await _tenantRepository.DidNotReceive().ListAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GlobalAdmin_ListaVazia_RetornaListaVazia()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        _tenantRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var result = await _sut.Handle(new ListAllTenantsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_GlobalAdmin_RetornaListaComCamposCorretos()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        var tenant = Tenant.Create("Clínica ABC").Value;
        tenant.AddMember(Guid.NewGuid(), TenantRole.Owner);
        tenant.AddMember(Guid.NewGuid(), TenantRole.Operator);
        _tenantRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });

        var result = await _sut.Handle(new ListAllTenantsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Clínica ABC");
        result.Value[0].IsActive.Should().BeTrue();
        result.Value[0].MemberCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_GlobalAdmin_TenantInativo_RetornaIsActiveFalse()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        var tenant = Tenant.Create("Clinic").Value;
        tenant.Deactivate();
        _tenantRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });

        var result = await _sut.Handle(new ListAllTenantsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].IsActive.Should().BeFalse();
    }
}
