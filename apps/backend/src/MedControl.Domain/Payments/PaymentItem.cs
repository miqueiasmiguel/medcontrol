using MedControl.Domain.Common;

namespace MedControl.Domain.Payments;

public sealed class PaymentItem : BaseEntity
{
    private PaymentItem() { } // EF Core

    public Guid PaymentId { get; private set; }
    public Guid ProcedureId { get; private set; }
    public decimal Value { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public static class Errors
    {
        public static readonly Error ValueInvalid = Error.Validation(
            "PaymentItem.ValueInvalid",
            "Payment item value must be greater than zero.");
    }

    internal static Result<PaymentItem> Create(Guid paymentId, Guid procedureId, decimal value, string? notes = null)
    {
        if (value <= 0)
        {
            return Result.Failure<PaymentItem>(Errors.ValueInvalid);
        }

        return new PaymentItem
        {
            PaymentId = paymentId,
            ProcedureId = procedureId,
            Value = value,
            Status = PaymentStatus.Pending,
            Notes = notes,
        };
    }

    public Result UpdateStatus(PaymentStatus status, string? notes = null)
    {
        Status = status;
        Notes = notes;
        return Result.Success();
    }
}
