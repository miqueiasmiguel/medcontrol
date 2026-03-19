using System.Text;
using FluentAssertions;
using MedControl.Domain.Procedures;
using MedControl.Infrastructure.Procedures.Parsers;

namespace MedControl.Infrastructure.Tests.Procedures;

public sealed class CbhpmCsvParserTests
{
    private readonly CbhpmCsvParser _sut = new();

    [Fact]
    public void Source_DeveSer_Cbhpm()
    {
        _sut.Source.Should().Be(ProcedureSource.Cbhpm);
    }

    [Fact]
    public void Parse_ComArquivoValido_DeveSomarPorteECusto()
    {
        var csv = """
            CÓDIGO;NOMENCLATURA;PORTE;CUSTO_OPERACIONAL
            10101012;Consulta em Clínica Médica;100,00;50,00
            20202025;Consulta Especializada;200,00;75,50
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().HaveCount(2);
        result.SkippedCount.Should().Be(0);
        result.Rows[0].Code.Should().Be("10101012");
        result.Rows[0].Description.Should().Be("Consulta em Clínica Médica");
        result.Rows[0].Value.Should().Be(150.00m);
        result.Rows[0].EffectiveTo.Should().BeNull();
        result.Rows[1].Value.Should().Be(275.50m);
    }

    [Fact]
    public void Parse_ComValorZeroCalculado_DevePularLinha()
    {
        var csv = """
            CÓDIGO;NOMENCLATURA;PORTE;CUSTO_OPERACIONAL
            10101012;Consulta em Clínica Médica;0,00;0,00
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().BeEmpty();
        result.SkippedCount.Should().Be(1);
    }

    [Fact]
    public void Parse_ComCustoInvalido_DevePularLinha()
    {
        var csv = """
            CÓDIGO;NOMENCLATURA;PORTE;CUSTO_OPERACIONAL
            10101012;Consulta em Clínica Médica;100,00;abc
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().BeEmpty();
        result.SkippedCount.Should().Be(1);
        result.ErrorSummary.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_ComColunasInsuficientes_DevePularLinha()
    {
        var csv = """
            CÓDIGO;NOMENCLATURA;PORTE;CUSTO_OPERACIONAL
            10101012;Consulta em Clínica Médica;100,00
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = _sut.Parse(stream);

        result.Rows.Should().BeEmpty();
        result.SkippedCount.Should().Be(1);
    }
}
