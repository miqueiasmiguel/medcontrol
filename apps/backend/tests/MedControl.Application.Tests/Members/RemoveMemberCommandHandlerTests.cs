using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.Commands.RemoveMember;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Members;

public sealed class RemoveMemberCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly RemoveMemberCommandHandler _sut;

    public RemoveMemberCommandHandlerTests()
    {
        _sut = new RemoveMemberCommandHandler(_tenantRepository, _unitOfWork, _currentTenant, _currentUser);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new RemoveMemberCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioSemPermissao_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _currentUser.Roles.Returns(new List<string> { "operator" });

        var result = await _sut.Handle(new RemoveMemberCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_MembroNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new RemoveMemberCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MembroOwner_DeveRetornarValidationError()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(userId, TenantRole.Owner);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new RemoveMemberCommand(userId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.OwnerCannotBeRemoved);
        result.Error.Type.Should().Be(ErrorType.Validation);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DadosValidos_DeveRemoverMembroESalvar()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(userId, TenantRole.Doctor);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new RemoveMemberCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Members.Should().BeEmpty();
        await _tenantRepository.Received(1).UpdateAsync(tenant, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
