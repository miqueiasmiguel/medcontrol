using FluentAssertions;
using MedControl.Application.Admin.Commands.SetTenantStatus;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Admin;

public sealed class SetTenantStatusCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly SetTenantStatusCommandHandler _sut;

    public SetTenantStatusCommandHandlerTests()
    {
        _sut = new SetTenantStatusCommandHandler(_currentUser, _tenantRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_NaoGlobalAdmin_RetornaUnauthorized()
    {
        _currentUser.HasGlobalRole("admin").Returns(false);

        var result = await _sut.Handle(new SetTenantStatusCommand(Guid.NewGuid(), false), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        await _tenantRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantNaoEncontrado_RetornaNotFound()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        _tenantRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(new SetTenantStatusCommand(Guid.NewGuid(), false), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_DesativarTenantAtivo_DesativaESalva()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        var tenant = Tenant.Create("Clinic").Value;
        _tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new SetTenantStatusCommand(tenant.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.IsActive.Should().BeFalse();
        await _tenantRepository.Received(1).UpdateAsync(tenant, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AtivarTenantInativo_AtivaESalva()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        var tenant = Tenant.Create("Clinic").Value;
        tenant.Deactivate();
        _tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new SetTenantStatusCommand(tenant.Id, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.IsActive.Should().BeTrue();
        await _tenantRepository.Received(1).UpdateAsync(tenant, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
