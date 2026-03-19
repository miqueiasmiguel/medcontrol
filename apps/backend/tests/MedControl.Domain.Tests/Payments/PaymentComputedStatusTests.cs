using FluentAssertions;
using MedControl.Domain.Payments;

namespace MedControl.Domain.Tests.Payments;

public sealed class PaymentComputedStatusTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static Payment CreatePayment(params (Guid ProcedureId, decimal Value)[] items)
    {
        return Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;
    }

    [Fact]
    public void Status_TodosPending_DeveSerPending()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));

        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Status_TodosPaid_DeveSerPaid()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));
        foreach (var item in payment.Items)
        {
            item.UpdateStatus(PaymentStatus.Paid);
        }

        payment.Status.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public void Status_TodosRefused_DeveSerRefused()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));
        foreach (var item in payment.Items)
        {
            item.UpdateStatus(PaymentStatus.Refused);
        }

        payment.Status.Should().Be(PaymentStatus.Refused);
    }

    [Fact]
    public void Status_PaidEPending_DeveSerPartiallyPending()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));
        payment.Items[0].UpdateStatus(PaymentStatus.Paid);

        payment.Status.Should().Be(PaymentStatus.PartiallyPending);
    }

    [Fact]
    public void Status_RefusedEPending_DeveSerPartiallyRefused()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));
        payment.Items[0].UpdateStatus(PaymentStatus.Refused);

        payment.Status.Should().Be(PaymentStatus.PartiallyRefused);
    }

    [Fact]
    public void Status_RefusedEPaid_DeveSerPartiallyRefused()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m));
        payment.Items[0].UpdateStatus(PaymentStatus.Refused);
        payment.Items[1].UpdateStatus(PaymentStatus.Paid);

        payment.Status.Should().Be(PaymentStatus.PartiallyRefused);
    }

    [Fact]
    public void Status_RefusedPaidEPending_DeveSerPartiallyRefused()
    {
        var payment = CreatePayment((Guid.NewGuid(), 100m), (Guid.NewGuid(), 200m), (Guid.NewGuid(), 300m));
        payment.Items[0].UpdateStatus(PaymentStatus.Refused);
        payment.Items[1].UpdateStatus(PaymentStatus.Paid);

        payment.Status.Should().Be(PaymentStatus.PartiallyRefused);
    }
}
