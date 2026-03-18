namespace MedControl.Application.Doctors.DTOs;

public sealed record DoctorDto(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string Name,
    string Crm,
    string CouncilState,
    string Specialty);
