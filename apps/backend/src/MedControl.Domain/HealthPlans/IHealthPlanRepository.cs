namespace MedControl.Domain.HealthPlans;

public interface IHealthPlanRepository
{
    Task<IReadOnlyList<HealthPlan>> ListAsync(CancellationToken ct = default);
    Task<HealthPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByTissCodeAsync(Guid tenantId, string tissCode, CancellationToken ct = default);
    Task AddAsync(HealthPlan healthPlan, CancellationToken ct = default);
    Task UpdateAsync(HealthPlan healthPlan, CancellationToken ct = default);
}
