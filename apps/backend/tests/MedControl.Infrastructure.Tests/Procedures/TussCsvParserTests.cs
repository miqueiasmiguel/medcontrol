using System.Text;
using FluentAssertions;
using MedControl.Domain.Procedures;
using MedControl.Infrastructure.Procedures.Parsers;

namespace MedControl.Infrastructure.Tests.Procedures;

public sealed class TussCsvParserTests
{
    private readonly TussCsvParser _sut = new();

    [Fact]
    public void Source_DeveSer_Tuss()
    {
        _sut.Source.Should().Be(ProcedureSource.Tuss);
    }

    [Fact]
    public void Parse_ComArquivoValido_DeveRetornarLinhas()
    {
        var csv = """
            CD_TUSS;DS_TERMO;VL_PORTE;DT_VIG_INICIO;DT_VIG_FIM
            10101012;Consulta em Clínica Médica;150,00;01/01/2025;31/12/2025
            20202025;Consulta Especializada;300,00;01/01/2025;
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().HaveCount(2);
        result.SkippedCount.Should().Be(0);
        result.Rows[0].Code.Should().Be("10101012");
        result.Rows[0].Description.Should().Be("Consulta em Clínica Médica");
        result.Rows[0].Value.Should().Be(150.00m);
        result.Rows[0].EffectiveTo.Should().Be(new DateOnly(2025, 12, 31));
        result.Rows[1].Code.Should().Be("20202025");
        result.Rows[1].EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void Parse_ComValorInvalido_DevePularLinha()
    {
        var csv = """
            CD_TUSS;DS_TERMO;VL_PORTE;DT_VIG_INICIO;DT_VIG_FIM
            10101012;Consulta em Clínica Médica;abc;01/01/2025;
            20202025;Consulta Especializada;300,00;01/01/2025;
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().HaveCount(1);
        result.SkippedCount.Should().Be(1);
        result.ErrorSummary.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_ComValorZero_DevePularLinha()
    {
        var csv = """
            CD_TUSS;DS_TERMO;VL_PORTE;DT_VIG_INICIO;DT_VIG_FIM
            10101012;Consulta em Clínica Médica;0;01/01/2025;
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().BeEmpty();
        result.SkippedCount.Should().Be(1);
    }

    [Fact]
    public void Parse_ComEffectiveToVazio_DeveRetornarEffectiveToNull()
    {
        var csv = """
            CD_TUSS;DS_TERMO;VL_PORTE;DT_VIG_INICIO;DT_VIG_FIM
            10101012;Consulta em Clínica Médica;150,00;01/01/2025;
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().HaveCount(1);
        result.Rows[0].EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void Parse_ComStreamVazia_DeveRetornarListaVazia()
    {
        var csv = "CD_TUSS;DS_TERMO;VL_PORTE;DT_VIG_INICIO;DT_VIG_FIM\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().BeEmpty();
        result.SkippedCount.Should().Be(0);
        result.ErrorSummary.Should().BeNull();
    }
}
