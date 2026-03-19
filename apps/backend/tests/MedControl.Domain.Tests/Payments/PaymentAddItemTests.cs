using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Domain.Tests.Payments;

public sealed class PaymentAddItemTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static Payment CreatePayment() =>
        Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null,
            [(Guid.NewGuid(), 150m)]).Value;

    [Fact]
    public void AddItem_ComValorValido_DeveAdicionarItemComStatusPending()
    {
        var payment = CreatePayment();
        var procedureId = Guid.NewGuid();

        var result = payment.AddItem(procedureId, 200m);

        result.IsSuccess.Should().BeTrue();
        payment.Items.Should().HaveCount(2);
        payment.Items[1].ProcedureId.Should().Be(procedureId);
        payment.Items[1].Value.Should().Be(200m);
        payment.Items[1].Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void AddItem_ComValueZero_DeveRetornarFalha()
    {
        var payment = CreatePayment();

        var result = payment.AddItem(Guid.NewGuid(), 0m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentItem.Errors.ValueInvalid);
        result.Error.Type.Should().Be(ErrorType.Validation);
        payment.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_ComValueNegativo_DeveRetornarFalha()
    {
        var payment = CreatePayment();

        var result = payment.AddItem(Guid.NewGuid(), -50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentItem.Errors.ValueInvalid);
        payment.Items.Should().HaveCount(1);
    }
}
