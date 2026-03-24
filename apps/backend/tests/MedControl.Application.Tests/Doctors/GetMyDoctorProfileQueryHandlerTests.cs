using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Queries.GetMyDoctorProfile;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using NSubstitute;

namespace MedControl.Application.Tests.Doctors;

public sealed class GetMyDoctorProfileQueryHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly GetMyDoctorProfileQueryHandler _sut;

    public GetMyDoctorProfileQueryHandlerTests()
    {
        _sut = new GetMyDoctorProfileQueryHandler(_doctorRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetMyDoctorProfileQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_SemPerfilVinculado_DeveRetornarNullComSucesso()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _doctorRepository.GetByCurrentUserAsync(userId, Arg.Any<CancellationToken>()).Returns((DoctorProfile?)null);

        var result = await _sut.Handle(new GetMyDoctorProfileQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComPerfilVinculado_DeveRetornarDoctorDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        doctor.LinkUser(userId);
        _currentUser.UserId.Returns(userId);
        _doctorRepository.GetByCurrentUserAsync(userId, Arg.Any<CancellationToken>()).Returns(doctor);

        var result = await _sut.Handle(new GetMyDoctorProfileQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Dr. João");
        result.Value.Crm.Should().Be("123456");
        result.Value.CouncilState.Should().Be("SP");
        result.Value.Specialty.Should().Be("Cardiologia");
        result.Value.UserId.Should().Be(userId);
    }
}
