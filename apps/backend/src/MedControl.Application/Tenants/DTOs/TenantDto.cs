namespace MedControl.Application.Tenants.DTOs;

public record TenantDto(Guid Id, string Name, string Slug, string Role);
