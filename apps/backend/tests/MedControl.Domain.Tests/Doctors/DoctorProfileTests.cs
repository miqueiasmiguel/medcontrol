using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Domain.Tests.Doctors;

public class DoctorProfileCreateTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveRetornarSucesso()
    {
        var tenantId = Guid.NewGuid();

        var result = DoctorProfile.Create(tenantId, "Dr. João Silva", "123456", "SP", "Cardiologia");

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Name.Should().Be("Dr. João Silva");
        result.Value.Crm.Should().Be("123456");
        result.Value.CouncilState.Should().Be("SP");
        result.Value.Specialty.Should().Be("Cardiologia");
        result.Value.UserId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComNomeInvalido_DeveRetornarFalha(string? name)
    {
        var result = DoctorProfile.Create(Guid.NewGuid(), name!, "123456", "SP", "Cardiologia");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DoctorProfile.Errors.NameRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComCrmInvalido_DeveRetornarFalha(string? crm)
    {
        var result = DoctorProfile.Create(Guid.NewGuid(), "Dr. João", crm!, "SP", "Cardiologia");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DoctorProfile.Errors.CrmRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComEstadoConselhoInvalido_DeveRetornarFalha(string? councilState)
    {
        var result = DoctorProfile.Create(Guid.NewGuid(), "Dr. João", "123456", councilState!, "Cardiologia");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DoctorProfile.Errors.CouncilStateRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComEspecialidadeInvalida_DeveRetornarFalha(string? specialty)
    {
        var result = DoctorProfile.Create(Guid.NewGuid(), "Dr. João", "123456", "SP", specialty!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DoctorProfile.Errors.SpecialtyRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}

public class DoctorProfileLinkUserTests
{
    [Fact]
    public void LinkUser_ComUserIdValido_DeveVincular()
    {
        var doctor = DoctorProfile.Create(Guid.NewGuid(), "Dr. João", "123456", "SP", "Cardiologia").Value;
        var userId = Guid.NewGuid();

        var result = doctor.LinkUser(userId);

        result.IsSuccess.Should().BeTrue();
        doctor.UserId.Should().Be(userId);
    }

    [Fact]
    public void LinkUser_QuandoJaVinculado_DeveRetornarConflict()
    {
        var doctor = DoctorProfile.Create(Guid.NewGuid(), "Dr. João", "123456", "SP", "Cardiologia").Value;
        var userId = Guid.NewGuid();
        doctor.LinkUser(userId);

        var result = doctor.LinkUser(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DoctorProfile.Errors.UserAlreadyLinked);
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
