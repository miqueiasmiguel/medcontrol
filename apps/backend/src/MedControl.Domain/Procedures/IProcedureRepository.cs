namespace MedControl.Domain.Procedures;

public interface IProcedureRepository
{
    Task<IReadOnlyList<Procedure>> ListAsync(CancellationToken ct = default);
    Task<Procedure?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task AddAsync(Procedure procedure, CancellationToken ct = default);
    Task UpdateAsync(Procedure procedure, CancellationToken ct = default);
}
