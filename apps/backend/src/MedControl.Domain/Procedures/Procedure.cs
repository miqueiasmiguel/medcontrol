using MedControl.Domain.Common;

namespace MedControl.Domain.Procedures;

public sealed class Procedure : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private Procedure() { } // EF Core

    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Value { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public ProcedureSource Source { get; private set; }

    public static class Errors
    {
        public static readonly Error CodeRequired = Error.Validation("Procedure.CodeRequired", "Procedure code is required.");
        public static readonly Error DescriptionRequired = Error.Validation("Procedure.DescriptionRequired", "Procedure description is required.");
        public static readonly Error ValueInvalid = Error.Validation("Procedure.ValueInvalid", "Procedure value must be greater than zero.");
        public static readonly Error EffectiveDateRangeInvalid = Error.Validation("Procedure.EffectiveDateRangeInvalid", "EffectiveTo must be greater than EffectiveFrom.");
    }

    public static Result<Procedure> Create(
        Guid tenantId,
        string code,
        string description,
        decimal value,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo = null,
        ProcedureSource source = ProcedureSource.Manual)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Procedure>(Errors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Procedure>(Errors.DescriptionRequired);
        }

        if (value <= 0)
        {
            return Result.Failure<Procedure>(Errors.ValueInvalid);
        }

        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
        {
            return Result.Failure<Procedure>(Errors.EffectiveDateRangeInvalid);
        }

        return new Procedure
        {
            TenantId = tenantId,
            Code = code.Trim(),
            Description = description.Trim(),
            Value = value,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Source = source,
        };
    }

    public Result Update(string code, string description, decimal value, DateOnly? effectiveTo = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure(Errors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(Errors.DescriptionRequired);
        }

        if (value <= 0)
        {
            return Result.Failure(Errors.ValueInvalid);
        }

        Code = code.Trim();
        Description = description.Trim();
        Value = value;
        EffectiveTo = effectiveTo;

        return Result.Success();
    }

    public void CloseVigencia(DateOnly closedOn)
    {
        EffectiveTo = closedOn;
    }
}
