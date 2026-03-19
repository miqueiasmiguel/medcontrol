namespace MedControl.Domain.Procedures;

public interface IProcedureImportRepository
{
    Task AddAsync(ProcedureImport import, CancellationToken ct = default);
    Task<IReadOnlyList<ProcedureImport>> ListAsync(CancellationToken ct = default);
}
