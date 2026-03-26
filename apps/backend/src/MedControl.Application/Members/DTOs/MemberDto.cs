namespace MedControl.Application.Members.DTOs;

public sealed record MemberDto(
    Guid UserId,
    string? DisplayName,
    string? Email,
    string? AvatarUrl,
    string Role,
    DateTimeOffset JoinedAt,
    bool Invited = false);
