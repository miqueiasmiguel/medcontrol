using MedControl.Domain.HealthPlans;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class HealthPlanRepository(ApplicationDbContext db) : IHealthPlanRepository
{
    public async Task<IReadOnlyList<HealthPlan>> ListAsync(CancellationToken ct = default) =>
        await db.HealthPlans.ToListAsync(ct);

    public Task<HealthPlan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.HealthPlans.FirstOrDefaultAsync(hp => hp.Id == id, ct);

    public Task<bool> ExistsByTissCodeAsync(Guid tenantId, string tissCode, CancellationToken ct = default) =>
        db.HealthPlans.AnyAsync(hp => hp.TenantId == tenantId && hp.TissCode == tissCode, ct);

    public async Task AddAsync(HealthPlan healthPlan, CancellationToken ct = default) =>
        await db.HealthPlans.AddAsync(healthPlan, ct);

    public Task UpdateAsync(HealthPlan healthPlan, CancellationToken ct = default)
    {
        db.HealthPlans.Update(healthPlan);
        return Task.CompletedTask;
    }
}
