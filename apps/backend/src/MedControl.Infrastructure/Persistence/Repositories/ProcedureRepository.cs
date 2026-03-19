using MedControl.Domain.Procedures;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class ProcedureRepository(ApplicationDbContext db) : IProcedureRepository
{
    public async Task<IReadOnlyList<Procedure>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = db.Procedures.AsQueryable();

        if (activeOnly)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(p =>
                p.EffectiveFrom <= today &&
                (p.EffectiveTo == null || p.EffectiveTo >= today));
        }

        return await query.ToListAsync(ct);
    }

    public Task<Procedure?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Procedures.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default) =>
        db.Procedures.AnyAsync(p => p.TenantId == tenantId && p.Code == code, ct);

    public Task<bool> ExistsByCodeAndEffectiveFromAsync(
        Guid tenantId, string code, DateOnly effectiveFrom, CancellationToken ct = default) =>
        db.Procedures.AnyAsync(
            p => p.TenantId == tenantId && p.Code == code && p.EffectiveFrom == effectiveFrom, ct);

    public async Task AddAsync(Procedure procedure, CancellationToken ct = default) =>
        await db.Procedures.AddAsync(procedure, ct);

    public async Task AddRangeAsync(IEnumerable<Procedure> procedures, CancellationToken ct = default) =>
        await db.Procedures.AddRangeAsync(procedures, ct);

    public Task UpdateAsync(Procedure procedure, CancellationToken ct = default)
    {
        db.Procedures.Update(procedure);
        return Task.CompletedTask;
    }
}
