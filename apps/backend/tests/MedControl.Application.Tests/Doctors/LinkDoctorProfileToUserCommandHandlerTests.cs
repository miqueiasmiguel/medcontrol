using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.LinkDoctorProfileToUser;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Doctors;

public sealed class LinkDoctorProfileToUserCommandHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly LinkDoctorProfileToUserCommandHandler _sut;

    public LinkDoctorProfileToUserCommandHandlerTests()
    {
        _sut = new LinkDoctorProfileToUserCommandHandler(
            _doctorRepository, _tenantRepository, _unitOfWork, _currentTenant, _currentUser);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioSemPermissao_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _currentUser.Roles.Returns(new List<string> { "operator" });

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_DoctorNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _doctorRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_UsuarioNaoEMembroDoctor_DeveRetornarValidationError()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var tenant = Tenant.Create("Clínica").Value;
        // userId is a member, but with operator role — NOT doctor
        tenant.AddMember(userId, TenantRole.Operator);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(doctor.Id, userId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_UsuarioNaoEMembro_DeveRetornarValidationError()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var tenant = Tenant.Create("Clínica").Value;
        // tenant has no members

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(doctor.Id, userId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_DoctorJaVinculado_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        doctor.LinkUser(existingUserId); // already linked
        var tenant = Tenant.Create("Clínica").Value;
        tenant.AddMember(newUserId, TenantRole.Doctor);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(doctor.Id, newUserId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _doctorRepository.DidNotReceive().UpdateAsync(Arg.Any<DoctorProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveVincularEPersistir()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var tenant = Tenant.Create("Clínica").Value;
        tenant.AddMember(userId, TenantRole.Doctor);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(doctor.Id, userId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Name.Should().Be("Dr. João");
        await _doctorRepository.Received(1).UpdateAsync(
            Arg.Is<DoctorProfile>(d => d.UserId == userId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnerTambemTemPermissao()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var tenant = Tenant.Create("Clínica").Value;
        tenant.AddMember(userId, TenantRole.Doctor);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "owner" });
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new LinkDoctorProfileToUserCommand(doctor.Id, userId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ComDoctorIdVazio_DeveRetornarErro()
    {
        var validator = new LinkDoctorProfileToUserCommandValidator();
        var validation = await validator.ValidateAsync(
            new LinkDoctorProfileToUserCommand(Guid.Empty, Guid.NewGuid()));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComUserIdVazio_DeveRetornarErro()
    {
        var validator = new LinkDoctorProfileToUserCommandValidator();
        var validation = await validator.ValidateAsync(
            new LinkDoctorProfileToUserCommand(Guid.NewGuid(), Guid.Empty));
        validation.IsValid.Should().BeFalse();
    }
}
