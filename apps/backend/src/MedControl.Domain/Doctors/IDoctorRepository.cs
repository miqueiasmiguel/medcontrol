namespace MedControl.Domain.Doctors;

public interface IDoctorRepository
{
    Task<IReadOnlyList<DoctorProfile>> ListAsync(CancellationToken ct = default);
    Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DoctorProfile?> GetByCurrentUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<DoctorProfile>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsByCrmAsync(Guid tenantId, string crm, string councilState, CancellationToken ct = default);
    Task<bool> ExistsByCrmInTenantAsync(Guid tenantId, string crm, string councilState, Guid excludeProfileId, CancellationToken ct = default);
    Task AddAsync(DoctorProfile doctor, CancellationToken ct = default);
    Task UpdateAsync(DoctorProfile doctor, CancellationToken ct = default);
}
