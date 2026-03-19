namespace MedControl.Domain.Payments;

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Refused = 2,
    PartiallyPending = 3,
    PartiallyRefused = 4,
}
