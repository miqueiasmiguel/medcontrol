using MedControl.Domain.Common;

namespace MedControl.Domain.HealthPlans;

public sealed class HealthPlan : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private HealthPlan() { } // EF Core

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string TissCode { get; private set; } = default!;

    public static class Errors
    {
        public static readonly Error NameRequired = Error.Validation("HealthPlan.NameRequired", "Health plan name is required.");
        public static readonly Error TissCodeRequired = Error.Validation("HealthPlan.TissCodeRequired", "TISS code is required.");
    }

    public static Result<HealthPlan> Create(Guid tenantId, string name, string tissCode)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<HealthPlan>(Errors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(tissCode))
        {
            return Result.Failure<HealthPlan>(Errors.TissCodeRequired);
        }

        return new HealthPlan
        {
            TenantId = tenantId,
            Name = name.Trim(),
            TissCode = tissCode.Trim(),
        };
    }

    public Result Update(string name, string tissCode)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Errors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(tissCode))
        {
            return Result.Failure(Errors.TissCodeRequired);
        }

        Name = name.Trim();
        TissCode = tissCode.Trim();

        return Result.Success();
    }
}
