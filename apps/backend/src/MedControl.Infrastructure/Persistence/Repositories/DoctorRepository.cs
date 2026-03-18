using MedControl.Domain.Doctors;
using MedControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence.Repositories;

internal sealed class DoctorRepository(ApplicationDbContext db) : IDoctorRepository
{
    public Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<bool> ExistsByCrmAsync(Guid tenantId, string crm, string councilState, CancellationToken ct = default) =>
        db.DoctorProfiles.AnyAsync(d => d.TenantId == tenantId && d.Crm == crm && d.CouncilState == councilState, ct);

    public async Task AddAsync(DoctorProfile doctor, CancellationToken ct = default) =>
        await db.DoctorProfiles.AddAsync(doctor, ct);

    public Task UpdateAsync(DoctorProfile doctor, CancellationToken ct = default)
    {
        db.DoctorProfiles.Update(doctor);
        return Task.CompletedTask;
    }
}
