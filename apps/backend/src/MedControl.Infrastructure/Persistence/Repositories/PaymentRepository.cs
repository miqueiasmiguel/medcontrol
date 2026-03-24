using MedControl.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(ApplicationDbContext db) : IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> ListAsync(PaymentFilters filters, CancellationToken ct = default)
    {
        var query = db.Payments.Include(p => p.Items).AsQueryable();

        if (filters.DoctorId.HasValue)
        {
            query = query.Where(p => p.DoctorId == filters.DoctorId.Value);
        }

        if (filters.HealthPlanId.HasValue)
        {
            query = query.Where(p => p.HealthPlanId == filters.HealthPlanId.Value);
        }

        if (filters.DateFrom.HasValue)
        {
            query = query.Where(p => p.ExecutionDate >= filters.DateFrom.Value);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(p => p.ExecutionDate <= filters.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            query = query.Where(p => EF.Functions.ILike(p.BeneficiaryName, $"%{filters.Search}%"));
        }

        query = (filters.SortBy, filters.SortDescending) switch
        {
            (PaymentSortBy.TotalValue, true) => query.OrderByDescending(p => p.Items.Sum(i => i.Value)),
            (PaymentSortBy.TotalValue, false) => query.OrderBy(p => p.Items.Sum(i => i.Value)),
            (_, true) => query.OrderByDescending(p => p.ExecutionDate),
            _ => query.OrderBy(p => p.ExecutionDate),
        };

        var payments = await query.ToListAsync(ct);

        if (filters.Status.HasValue)
        {
            return payments.Where(p => p.Status == filters.Status.Value).ToList();
        }

        return payments;
    }

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
