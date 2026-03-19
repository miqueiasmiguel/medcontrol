namespace MedControl.Domain.Procedures;

public interface IProcedureRepository
{
    Task<IReadOnlyList<Procedure>> ListAsync(bool activeOnly, CancellationToken ct = default);
    Task<Procedure?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<bool> ExistsByCodeAndEffectiveFromAsync(Guid tenantId, string code, DateOnly effectiveFrom, CancellationToken ct = default);
    Task AddAsync(Procedure procedure, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Procedure> procedures, CancellationToken ct = default);
    Task UpdateAsync(Procedure procedure, CancellationToken ct = default);
}
