namespace MedControl.Domain.Payments;

public record PaymentFilters(
    Guid? DoctorId = null,
    Guid? HealthPlanId = null,
    PaymentStatus? Status = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? Search = null,
    PaymentSortBy SortBy = PaymentSortBy.ExecutionDate,
    bool SortDescending = true);

public enum PaymentSortBy
{
    ExecutionDate,
    TotalValue,
}
