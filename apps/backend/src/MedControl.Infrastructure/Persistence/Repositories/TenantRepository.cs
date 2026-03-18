using MedControl.Domain.Tenants;
using MedControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository(ApplicationDbContext db) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tenants.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public async Task<IReadOnlyList<Tenant>> ListByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tenantIds = await db.TenantMembers
            .IgnoreQueryFilters()
            .Where(m => m.UserId == userId)
            .Select(m => m.TenantId)
            .ToListAsync(ct);

        return await db.Tenants
            .IgnoreQueryFilters()
            .Include(t => t.Members)
            .Where(t => tenantIds.Contains(t.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await db.Tenants.AddAsync(tenant, ct);

    public Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        db.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}
