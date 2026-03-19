using MedControl.Domain.Procedures;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class ProcedureImportRepository(ApplicationDbContext db) : IProcedureImportRepository
{
    public async Task AddAsync(ProcedureImport import, CancellationToken ct = default) =>
        await db.ProcedureImports.AddAsync(import, ct);

    public async Task<IReadOnlyList<ProcedureImport>> ListAsync(CancellationToken ct = default) =>
        await db.ProcedureImports.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
}
