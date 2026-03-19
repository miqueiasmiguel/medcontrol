using MedControl.Domain.Common;

namespace MedControl.Domain.Payments;

public sealed class Payment : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private readonly List<PaymentItem> _items = [];

    private Payment() { } // EF Core

    public Guid TenantId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid HealthPlanId { get; private set; }
    public DateOnly ExecutionDate { get; private set; }
    public string AppointmentNumber { get; private set; } = default!;
    public string? AuthorizationCode { get; private set; }
    public string BeneficiaryCard { get; private set; } = default!;
    public string BeneficiaryName { get; private set; } = default!;
    public string ExecutionLocation { get; private set; } = default!;
    public string PaymentLocation { get; private set; } = default!;
    public string? Notes { get; private set; }

    public IReadOnlyList<PaymentItem> Items => _items.AsReadOnly();

    public static class Errors
    {
        public static readonly Error AppointmentNumberRequired = Error.Validation(
            "Payment.AppointmentNumberRequired",
            "Appointment number is required.");

        public static readonly Error BeneficiaryCardRequired = Error.Validation(
            "Payment.BeneficiaryCardRequired",
            "Beneficiary card is required.");

        public static readonly Error BeneficiaryNameRequired = Error.Validation(
            "Payment.BeneficiaryNameRequired",
            "Beneficiary name is required.");

        public static readonly Error ExecutionLocationRequired = Error.Validation(
            "Payment.ExecutionLocationRequired",
            "Execution location is required.");

        public static readonly Error PaymentLocationRequired = Error.Validation(
            "Payment.PaymentLocationRequired",
            "Payment location is required.");

        public static readonly Error ItemsRequired = Error.Validation(
            "Payment.ItemsRequired",
            "At least one payment item is required.");

        public static readonly Error ItemNotFound = Error.NotFound(
            "Payment.ItemNotFound",
            "Payment item not found.");
    }

    public static Result<Payment> Create(
        Guid tenantId,
        Guid doctorId,
        Guid healthPlanId,
        DateOnly executionDate,
        string appointmentNumber,
        string? authorizationCode,
        string beneficiaryCard,
        string beneficiaryName,
        string executionLocation,
        string paymentLocation,
        string? notes,
        IEnumerable<(Guid ProcedureId, decimal Value)> items)
    {
        if (string.IsNullOrWhiteSpace(appointmentNumber))
        {
            return Result.Failure<Payment>(Errors.AppointmentNumberRequired);
        }

        if (string.IsNullOrWhiteSpace(beneficiaryCard))
        {
            return Result.Failure<Payment>(Errors.BeneficiaryCardRequired);
        }

        if (string.IsNullOrWhiteSpace(beneficiaryName))
        {
            return Result.Failure<Payment>(Errors.BeneficiaryNameRequired);
        }

        if (string.IsNullOrWhiteSpace(executionLocation))
        {
            return Result.Failure<Payment>(Errors.ExecutionLocationRequired);
        }

        if (string.IsNullOrWhiteSpace(paymentLocation))
        {
            return Result.Failure<Payment>(Errors.PaymentLocationRequired);
        }

        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return Result.Failure<Payment>(Errors.ItemsRequired);
        }

        var payment = new Payment
        {
            TenantId = tenantId,
            DoctorId = doctorId,
            HealthPlanId = healthPlanId,
            ExecutionDate = executionDate,
            AppointmentNumber = appointmentNumber.Trim(),
            AuthorizationCode = authorizationCode?.Trim(),
            BeneficiaryCard = beneficiaryCard.Trim(),
            BeneficiaryName = beneficiaryName.Trim(),
            ExecutionLocation = executionLocation.Trim(),
            PaymentLocation = paymentLocation.Trim(),
            Notes = notes?.Trim(),
        };

        foreach (var (procedureId, value) in itemList)
        {
            var itemResult = PaymentItem.Create(payment.Id, procedureId, value);
            if (itemResult.IsFailure)
            {
                return Result.Failure<Payment>(itemResult.Error);
            }

            payment._items.Add(itemResult.Value);
        }

        return payment;
    }

    public Result<PaymentItem> GetItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            return Result.Failure<PaymentItem>(Errors.ItemNotFound);
        }

        return item;
    }
}
