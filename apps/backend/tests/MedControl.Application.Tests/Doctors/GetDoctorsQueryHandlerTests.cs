using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Queries.GetDoctors;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using NSubstitute;

namespace MedControl.Application.Tests.Doctors;

public sealed class GetDoctorsQueryHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly GetDoctorsQueryHandler _sut;

    public GetDoctorsQueryHandlerTests()
    {
        _sut = new GetDoctorsQueryHandler(_doctorRepository, _currentTenant);
    }

    [Fact]
    public async Task Handle_SemTenantContext_RetornaUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetDoctorsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ComTenantValido_RetornaListaDeMedicos()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);

        var doctor = DoctorProfile.Create(tenantId, "Dr. João Silva", "123456", "SP", "Cardiologia").Value!;
        _doctorRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DoctorProfile> { doctor });

        var result = await _sut.Handle(new GetDoctorsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value![0].Name.Should().Be("Dr. João Silva");
        result.Value![0].Crm.Should().Be("123456");
        result.Value![0].CouncilState.Should().Be("SP");
        result.Value![0].Specialty.Should().Be("Cardiologia");
    }

    [Fact]
    public async Task Handle_SemMedicos_RetornaListaVazia()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DoctorProfile>());

        var result = await _sut.Handle(new GetDoctorsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
