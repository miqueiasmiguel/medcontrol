using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Domain.Tests.Payments;

public sealed class PaymentStatusTests
{
    [Fact]
    public void PaymentStatus_DeveTerValoresPendingPaidRefused()
    {
        ((int)PaymentStatus.Pending).Should().Be(0);
        ((int)PaymentStatus.Paid).Should().Be(1);
        ((int)PaymentStatus.Refused).Should().Be(2);
    }
}

public sealed class PaymentCreateTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly List<(Guid ProcedureId, decimal Value)> ValidItems =
        [(Guid.NewGuid(), 150.00m)];

    [Fact]
    public void Create_ComCamposValidos_DeveCriarComItemsPendentes()
    {
        var tenantId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var healthPlanId = Guid.NewGuid();

        var result = Payment.Create(
            tenantId, doctorId, healthPlanId, Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, ValidItems);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.DoctorId.Should().Be(doctorId);
        result.Value.HealthPlanId.Should().Be(healthPlanId);
        result.Value.ExecutionDate.Should().Be(Today);
        result.Value.AppointmentNumber.Should().Be("ATD-001");
        result.Value.BeneficiaryCard.Should().Be("123456789");
        result.Value.BeneficiaryName.Should().Be("João Silva");
        result.Value.ExecutionLocation.Should().Be("Hospital ABC");
        result.Value.PaymentLocation.Should().Be("Convênio XYZ");
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be(PaymentStatus.Pending);
        result.Value.Items[0].Value.Should().Be(150.00m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComAppointmentNumberInvalido_DeveRetornarFalha(string? appointmentNumber)
    {
        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            appointmentNumber!, null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, ValidItems);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.AppointmentNumberRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComBeneficiaryCardInvalido_DeveRetornarFalha(string? beneficiaryCard)
    {
        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, beneficiaryCard!, "João Silva",
            "Hospital ABC", "Convênio XYZ", null, ValidItems);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.BeneficiaryCardRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComBeneficiaryNameInvalido_DeveRetornarFalha(string? beneficiaryName)
    {
        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", beneficiaryName!,
            "Hospital ABC", "Convênio XYZ", null, ValidItems);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.BeneficiaryNameRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComExecutionLocationInvalido_DeveRetornarFalha(string? executionLocation)
    {
        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            executionLocation!, "Convênio XYZ", null, ValidItems);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.ExecutionLocationRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComPaymentLocationInvalido_DeveRetornarFalha(string? paymentLocation)
    {
        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", paymentLocation!, null, ValidItems);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.PaymentLocationRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_ComListaDeItensVazia_DeveRetornarFalha()
    {
        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null,
            Array.Empty<(Guid, decimal)>());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.ItemsRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_ComItemComValueZero_DeveRetornarFalha()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 0m) };

        var result = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentItem.Errors.ValueInvalid);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}

public sealed class PaymentGetItemTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void GetItem_QuandoItemExiste_DeveRetornarItem()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        var itemId = payment.Items[0].Id;
        var result = payment.GetItem(itemId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(itemId);
    }

    [Fact]
    public void GetItem_QuandoItemNaoExiste_DeveRetornarNotFound()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        var result = payment.GetItem(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.ItemNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}

public sealed class PaymentItemUpdateStatusTests
{
    [Fact]
    public void UpdateStatus_DeveAlterarStatusENotes()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        var item = payment.Items[0];
        item.Status.Should().Be(PaymentStatus.Pending);

        var result = item.UpdateStatus(PaymentStatus.Paid, "Pago via transferência");

        result.IsSuccess.Should().BeTrue();
        item.Status.Should().Be(PaymentStatus.Paid);
        item.Notes.Should().Be("Pago via transferência");
    }

    [Fact]
    public void UpdateStatus_ParaRefused_DeveFuncionar()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        var item = payment.Items[0];
        var result = item.UpdateStatus(PaymentStatus.Refused, "Glosa por falta de autorização");

        result.IsSuccess.Should().BeTrue();
        item.Status.Should().Be(PaymentStatus.Refused);
        item.Notes.Should().Be("Glosa por falta de autorização");
    }
}
