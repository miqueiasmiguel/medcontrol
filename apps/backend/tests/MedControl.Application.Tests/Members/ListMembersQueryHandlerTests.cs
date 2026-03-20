using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.Queries.ListMembers;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Members;

public sealed class ListMembersQueryHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ListMembersQueryHandler _sut;

    public ListMembersQueryHandlerTests()
    {
        _sut = new ListMembersQueryHandler(_tenantRepository, _userRepository, _currentTenant);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new ListMembersQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_TenantNaoEncontrado_DeveRetornarNotFound()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _tenantRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(new ListMembersQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_TenantSemMembros_DeveRetornarListaVazia()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _userRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<User>().AsReadOnly() as IReadOnlyList<User>);

        var result = await _sut.Handle(new ListMembersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ComMembros_DeveBuscarUsuariosERetornarDtos()
    {
        var tenantId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(userId1, TenantRole.Admin);
        tenant.AddMember(userId2, TenantRole.Doctor);

        var user1 = User.Create("admin@example.com", "Admin User").Value;
        var user2 = User.Create("doctor@example.com", "Doctor User").Value;

        _currentTenant.TenantId.Returns(tenantId);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _userRepository.GetByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(userId1) && ids.Contains(userId2)),
            Arg.Any<CancellationToken>())
            .Returns(new List<User> { user1, user2 }.AsReadOnly() as IReadOnlyList<User>);

        var result = await _sut.Handle(new ListMembersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        await _userRepository.Received(1).GetByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(userId1) && ids.Contains(userId2)),
            Arg.Any<CancellationToken>());
    }
}
