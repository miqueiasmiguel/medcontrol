using MedControl.Domain.Procedures;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class ProcedureRepository(ApplicationDbContext db) : IProcedureRepository
{
    public async Task<IReadOnlyList<Procedure>> ListAsync(CancellationToken ct = default) =>
        await db.Procedures.ToListAsync(ct);

    public Task<Procedure?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Procedures.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default) =>
        db.Procedures.AnyAsync(p => p.TenantId == tenantId && p.Code == code, ct);

    public async Task AddAsync(Procedure procedure, CancellationToken ct = default) =>
        await db.Procedures.AddAsync(procedure, ct);

    public Task UpdateAsync(Procedure procedure, CancellationToken ct = default)
    {
        db.Procedures.Update(procedure);
        return Task.CompletedTask;
    }
}
