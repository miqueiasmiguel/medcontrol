using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Procedures.Queries.GetProcedureImports;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;
using NSubstitute;

namespace MedControl.Application.Tests.Procedures;

public sealed class GetProcedureImportsQueryHandlerTests
{
    private readonly IProcedureImportRepository _importRepository = Substitute.For<IProcedureImportRepository>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly GetProcedureImportsQueryHandler _sut;

    public GetProcedureImportsQueryHandlerTests()
    {
        _sut = new GetProcedureImportsQueryHandler(_importRepository, _currentTenant);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetProcedureImportsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ComTenant_DeveRetornarListaMapeada()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var import = ProcedureImport.Create(tenantId, ProcedureSource.Tuss, new DateOnly(2025, 1, 1), 100, 95, 5, null).Value;
        _importRepository.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<ProcedureImport> { import });

        var result = await _sut.Handle(new GetProcedureImportsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Source.Should().Be("Tuss");
        result.Value[0].TotalRows.Should().Be(100);
        result.Value[0].ImportedRows.Should().Be(95);
        result.Value[0].SkippedRows.Should().Be(5);
    }
}
