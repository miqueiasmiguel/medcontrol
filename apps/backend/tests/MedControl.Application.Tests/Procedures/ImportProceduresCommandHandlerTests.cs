using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Procedures.Commands.ImportProcedures;
using MedControl.Application.Procedures.Parsers;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;
using NSubstitute;

namespace MedControl.Application.Tests.Procedures;

public sealed class ImportProceduresCommandHandlerTests
{
    private readonly IProcedureRepository _procedureRepository = Substitute.For<IProcedureRepository>();
    private readonly IProcedureImportRepository _importRepository = Substitute.For<IProcedureImportRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly IProcedureFileParser _tussParser = Substitute.For<IProcedureFileParser>();

    private readonly ImportProceduresCommandHandler _sut;

    private static readonly DateOnly EffectiveFrom = new(2025, 1, 1);

    public ImportProceduresCommandHandlerTests()
    {
        _tussParser.Source.Returns(ProcedureSource.Tuss);
        _sut = new ImportProceduresCommandHandler(
            new[] { _tussParser },
            _procedureRepository,
            _importRepository,
            _unitOfWork,
            _currentTenant);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new ImportProceduresCommand(Stream.Null, ProcedureSource.Tuss, EffectiveFrom);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ComArquivoValido_DeveImportarProcedimentos()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var rows = new List<ParsedProcedureRow>
        {
            new("10101012", "Consulta médica", 150.00m, null),
            new("20202025", "Consulta especializada", 300.00m, null),
        };
        _tussParser.Parse(Arg.Any<Stream>()).Returns(new ParseResult(rows, 0, null));
        _procedureRepository.ExistsByCodeAndEffectiveFromAsync(
            tenantId, Arg.Any<string>(), EffectiveFrom, Arg.Any<CancellationToken>()).Returns(false);

        var command = new ImportProceduresCommand(Stream.Null, ProcedureSource.Tuss, EffectiveFrom);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedRows.Should().Be(2);
        result.Value.SkippedRows.Should().Be(0);
        result.Value.Source.Should().Be("Tuss");
        await _procedureRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Procedure>>(list => list.Count() == 2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComDuplicados_DeveContabilizarComoSkipped()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var rows = new List<ParsedProcedureRow>
        {
            new("10101012", "Consulta médica", 150.00m, null),
            new("20202025", "Consulta especializada", 300.00m, null),
        };
        _tussParser.Parse(Arg.Any<Stream>()).Returns(new ParseResult(rows, 0, null));
        _procedureRepository.ExistsByCodeAndEffectiveFromAsync(
            tenantId, "10101012", EffectiveFrom, Arg.Any<CancellationToken>()).Returns(true);
        _procedureRepository.ExistsByCodeAndEffectiveFromAsync(
            tenantId, "20202025", EffectiveFrom, Arg.Any<CancellationToken>()).Returns(false);

        var command = new ImportProceduresCommand(Stream.Null, ProcedureSource.Tuss, EffectiveFrom);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedRows.Should().Be(1);
        result.Value.SkippedRows.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ComParserNaoEncontrado_DeveRetornarValidationError()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);

        var command = new ImportProceduresCommand(Stream.Null, ProcedureSource.Cbhpm, EffectiveFrom);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
