using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Application.Doctors.Commands.UpdateMyDoctorProfile;

public sealed class UpdateMyDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateMyDoctorProfileCommand, Result<IReadOnlyList<DoctorDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "Authentication is required.");

    private static readonly Error NotFound =
        Error.NotFound("Doctor.NotFound", "No doctor profile found for the current user.");

    private static readonly Error CrmAlreadyExists =
        Error.Conflict("Doctor.CrmAlreadyExists", "A doctor with this CRM and council state already exists in this tenant.");

    public async Task<Result<IReadOnlyList<DoctorDto>>> Handle(UpdateMyDoctorProfileCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<IReadOnlyList<DoctorDto>>(Unauthorized);
        }

        var profiles = await doctorRepository.GetAllByUserIdAsync(currentUser.UserId.Value, ct);
        if (profiles.Count == 0)
        {
            return Result.Failure<IReadOnlyList<DoctorDto>>(NotFound);
        }

        foreach (var profile in profiles)
        {
            var crmChanged = profile.Crm != request.Crm.Trim() || profile.CouncilState != request.CouncilState.Trim();
            if (crmChanged)
            {
                var alreadyExists = await doctorRepository.ExistsByCrmInTenantAsync(
                    profile.TenantId, request.Crm, request.CouncilState, profile.Id, ct);
                if (alreadyExists)
                {
                    return Result.Failure<IReadOnlyList<DoctorDto>>(CrmAlreadyExists);
                }
            }

            var updateResult = profile.Update(request.Name, request.Crm, request.CouncilState, request.Specialty);
            if (updateResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<DoctorDto>>(updateResult.Error);
            }

            await doctorRepository.UpdateAsync(profile, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        var dtos = profiles.Select(p => new DoctorDto(
            p.Id, p.TenantId, p.UserId, p.Name, p.Crm, p.CouncilState, p.Specialty))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<DoctorDto>>(dtos);
    }
}
