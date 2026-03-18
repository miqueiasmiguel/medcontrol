namespace MedControl.Application.HealthPlans.DTOs;

public sealed record HealthPlanDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string TissCode);
