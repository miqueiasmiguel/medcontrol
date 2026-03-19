using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Domain.Tests.Payments;

public sealed class PaymentUpdateInfoTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static Payment CreatePayment() =>
        Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null,
            [(Guid.NewGuid(), 150m)]).Value;

    [Fact]
    public void Update_ComCamposValidos_DeveAtualizarTodasAsPropriedades()
    {
        var payment = CreatePayment();
        var newDate = Today.AddDays(1);

        var result = payment.Update(newDate, "ATD-002", "AUTH-999", "987654321",
            "Maria Santos", "Hospital XYZ", "Convênio ABC", "obs");

        result.IsSuccess.Should().BeTrue();
        payment.ExecutionDate.Should().Be(newDate);
        payment.AppointmentNumber.Should().Be("ATD-002");
        payment.AuthorizationCode.Should().Be("AUTH-999");
        payment.BeneficiaryCard.Should().Be("987654321");
        payment.BeneficiaryName.Should().Be("Maria Santos");
        payment.ExecutionLocation.Should().Be("Hospital XYZ");
        payment.PaymentLocation.Should().Be("Convênio ABC");
        payment.Notes.Should().Be("obs");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComAppointmentNumberVazio_DeveRetornarFalha(string? appointmentNumber)
    {
        var payment = CreatePayment();

        var result = payment.Update(Today, appointmentNumber!, null, "123456789",
            "João Silva", "Hospital ABC", "Convênio XYZ", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.AppointmentNumberRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComBeneficiaryCardVazio_DeveRetornarFalha(string? beneficiaryCard)
    {
        var payment = CreatePayment();

        var result = payment.Update(Today, "ATD-001", null, beneficiaryCard!,
            "João Silva", "Hospital ABC", "Convênio XYZ", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.BeneficiaryCardRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComBeneficiaryNameVazio_DeveRetornarFalha(string? beneficiaryName)
    {
        var payment = CreatePayment();

        var result = payment.Update(Today, "ATD-001", null, "123456789",
            beneficiaryName!, "Hospital ABC", "Convênio XYZ", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.BeneficiaryNameRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComExecutionLocationVazio_DeveRetornarFalha(string? executionLocation)
    {
        var payment = CreatePayment();

        var result = payment.Update(Today, "ATD-001", null, "123456789",
            "João Silva", executionLocation!, "Convênio XYZ", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.ExecutionLocationRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComPaymentLocationVazio_DeveRetornarFalha(string? paymentLocation)
    {
        var payment = CreatePayment();

        var result = payment.Update(Today, "ATD-001", null, "123456789",
            "João Silva", "Hospital ABC", paymentLocation!, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.PaymentLocationRequired);
    }
}
