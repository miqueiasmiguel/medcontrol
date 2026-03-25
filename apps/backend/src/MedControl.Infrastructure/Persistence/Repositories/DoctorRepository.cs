using MedControl.Domain.Doctors;
using MedControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class DoctorRepository(ApplicationDbContext db) : IDoctorRepository
{
    public async Task<IReadOnlyList<DoctorProfile>> ListAsync(CancellationToken ct = default) =>
        await db.DoctorProfiles.ToListAsync(ct);

    public Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<DoctorProfile?> GetByCurrentUserAsync(Guid userId, CancellationToken ct = default) =>
        db.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId, ct);

    public async Task<IReadOnlyList<DoctorProfile>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await db.DoctorProfiles
            .IgnoreQueryFilters()
            .Where(d => d.UserId == userId)
            .ToListAsync(ct);

    public Task<bool> ExistsByCrmAsync(Guid tenantId, string crm, string councilState, CancellationToken ct = default) =>
        db.DoctorProfiles.AnyAsync(d => d.TenantId == tenantId && d.Crm == crm && d.CouncilState == councilState, ct);

    public Task<bool> ExistsByCrmInTenantAsync(Guid tenantId, string crm, string councilState, Guid excludeProfileId, CancellationToken ct = default) =>
        db.DoctorProfiles
            .IgnoreQueryFilters()
            .AnyAsync(d => d.TenantId == tenantId && d.Crm == crm && d.CouncilState == councilState && d.Id != excludeProfileId, ct);

    public async Task AddAsync(DoctorProfile doctor, CancellationToken ct = default) =>
        await db.DoctorProfiles.AddAsync(doctor, ct);

    public Task UpdateAsync(DoctorProfile doctor, CancellationToken ct = default)
    {
        db.DoctorProfiles.Update(doctor);
        return Task.CompletedTask;
    }
}
