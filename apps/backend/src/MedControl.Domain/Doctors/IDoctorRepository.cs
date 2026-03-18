namespace MedControl.Domain.Doctors;

public interface IDoctorRepository
{
    Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCrmAsync(Guid tenantId, string crm, string councilState, CancellationToken ct = default);
    Task AddAsync(DoctorProfile doctor, CancellationToken ct = default);
    Task UpdateAsync(DoctorProfile doctor, CancellationToken ct = default);
}
