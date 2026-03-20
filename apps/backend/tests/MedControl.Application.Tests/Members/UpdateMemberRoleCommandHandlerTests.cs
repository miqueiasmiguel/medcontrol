using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.Commands.UpdateMemberRole;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Members;

public sealed class UpdateMemberRoleCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly UpdateMemberRoleCommandHandler _sut;

    public UpdateMemberRoleCommandHandlerTests()
    {
        _sut = new UpdateMemberRoleCommandHandler(_tenantRepository, _userRepository, _unitOfWork, _currentTenant, _currentUser);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new UpdateMemberRoleCommand(Guid.NewGuid(), "doctor"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioSemPermissao_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _currentUser.Roles.Returns(new List<string> { "operator" });

        var result = await _sut.Handle(new UpdateMemberRoleCommand(Guid.NewGuid(), "doctor"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_AtualizandoProprioRole_DeveRetornarCannotUpdateOwnRole()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(userId, TenantRole.Operator);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _currentUser.UserId.Returns(userId);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new UpdateMemberRoleCommand(userId, "doctor"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.CannotUpdateOwnRole);
    }

    [Fact]
    public async Task Handle_MembroNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _currentUser.UserId.Returns(Guid.NewGuid());
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new UpdateMemberRoleCommand(Guid.NewGuid(), "doctor"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_DadosValidos_DeveAtualizarRoleERetornarDto()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(userId, TenantRole.Operator);

        var user = User.Create("member@example.com", "Member").Value;

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _currentUser.UserId.Returns(currentUserId);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.Handle(new UpdateMemberRoleCommand(userId, "doctor"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("doctor");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
