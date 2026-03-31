namespace MedControl.Application.Admin.DTOs;

public sealed record AdminTenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTimeOffset CreatedAt,
    int MemberCount);
