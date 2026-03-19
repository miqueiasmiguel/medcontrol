using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Procedures.Queries.GetProcedures;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;
using NSubstitute;

namespace MedControl.Application.Tests.Procedures;

public sealed class GetProceduresQueryHandlerTests
{
    private readonly IProcedureRepository _procedureRepository = Substitute.For<IProcedureRepository>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly GetProceduresQueryHandler _sut;

    public GetProceduresQueryHandlerTests()
    {
        _sut = new GetProceduresQueryHandler(_procedureRepository, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComTenant_DeveRetornarListaMapeada()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var procedure = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m).Value;
        _procedureRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Procedure> { procedure });

        var result = await _sut.Handle(new GetProceduresQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Code.Should().Be("10101012");
        result.Value[0].Description.Should().Be("Consulta médica");
        result.Value[0].Value.Should().Be(150.00m);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetProceduresQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
