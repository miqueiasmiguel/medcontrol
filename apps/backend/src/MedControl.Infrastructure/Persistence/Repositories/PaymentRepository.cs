using MedControl.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(ApplicationDbContext db) : IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> ListAsync(CancellationToken ct = default) =>
        await db.Payments.Include(p => p.Items).ToListAsync(ct);

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Payments.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default) =>
        await db.Payments.AddAsync(payment, ct);

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        return Task.CompletedTask;
    }
}
