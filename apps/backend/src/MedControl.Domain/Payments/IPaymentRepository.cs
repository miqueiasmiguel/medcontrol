namespace MedControl.Domain.Payments;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> ListAsync(PaymentFilters filters, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
}
