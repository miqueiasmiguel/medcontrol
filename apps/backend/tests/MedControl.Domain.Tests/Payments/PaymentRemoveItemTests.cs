using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Domain.Tests.Payments;

public sealed class PaymentRemoveItemTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static Payment CreatePayment(params (Guid ProcedureId, decimal Value)[] items) =>
        Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

    [Fact]
    public void RemoveItem_Com2Itens_DeveRemoverUmERetornarSucesso()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));
        var itemId = payment.Items[0].Id;

        var result = payment.RemoveItem(itemId);

        result.IsSuccess.Should().BeTrue();
        payment.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_Com1Item_DeveRetornarMinimumItemsRequired()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m));
        var itemId = payment.Items[0].Id;

        var result = payment.RemoveItem(itemId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.MinimumItemsRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
        payment.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_ComItemInexistente_DeveRetornarItemNotFound()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));

        var result = payment.RemoveItem(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Payment.Errors.ItemNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
        payment.Items.Should().HaveCount(2);
    }
}
