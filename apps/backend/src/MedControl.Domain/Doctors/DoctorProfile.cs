using MedControl.Domain.Common;

namespace MedControl.Domain.Doctors;

public sealed class DoctorProfile : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private DoctorProfile() { } // EF Core

    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Crm { get; private set; } = default!;
    public string CouncilState { get; private set; } = default!;
    public string Specialty { get; private set; } = default!;

    public static class Errors
    {
        public static readonly Error NameRequired = Error.Validation("DoctorProfile.NameRequired", "Doctor name is required.");
        public static readonly Error CrmRequired = Error.Validation("DoctorProfile.CrmRequired", "CRM is required.");
        public static readonly Error CouncilStateRequired = Error.Validation("DoctorProfile.CouncilStateRequired", "Council state is required.");
        public static readonly Error SpecialtyRequired = Error.Validation("DoctorProfile.SpecialtyRequired", "Specialty is required.");
        public static readonly Error UserAlreadyLinked = Error.Conflict("DoctorProfile.UserAlreadyLinked", "A user is already linked to this doctor profile.");
        public static readonly Error OnlyLinkedDoctorCanUpdate = Error.Forbidden("DoctorProfile.OnlyLinkedDoctorCanUpdate", "Only the linked doctor user can update this profile.");
    }

    public static Result<DoctorProfile> Create(Guid tenantId, string name, string crm, string councilState, string specialty)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<DoctorProfile>(Errors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(crm))
        {
            return Result.Failure<DoctorProfile>(Errors.CrmRequired);
        }

        if (string.IsNullOrWhiteSpace(councilState))
        {
            return Result.Failure<DoctorProfile>(Errors.CouncilStateRequired);
        }

        if (string.IsNullOrWhiteSpace(specialty))
        {
            return Result.Failure<DoctorProfile>(Errors.SpecialtyRequired);
        }

        return new DoctorProfile
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Crm = crm.Trim(),
            CouncilState = councilState.Trim(),
            Specialty = specialty.Trim(),
        };
    }

    public Result Update(string name, string crm, string councilState, string specialty)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Errors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(crm))
        {
            return Result.Failure(Errors.CrmRequired);
        }

        if (string.IsNullOrWhiteSpace(councilState))
        {
            return Result.Failure(Errors.CouncilStateRequired);
        }

        if (string.IsNullOrWhiteSpace(specialty))
        {
            return Result.Failure(Errors.SpecialtyRequired);
        }

        Name = name.Trim();
        Crm = crm.Trim();
        CouncilState = councilState.Trim();
        Specialty = specialty.Trim();

        return Result.Success();
    }

    public Result LinkUser(Guid userId)
    {
        if (UserId.HasValue)
        {
            return Result.Failure(Errors.UserAlreadyLinked);
        }

        UserId = userId;
        return Result.Success();
    }
}
