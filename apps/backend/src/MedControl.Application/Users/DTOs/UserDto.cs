namespace MedControl.Application.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? AvatarUrl,
    bool IsEmailVerified,
    string GlobalRole,
    DateTimeOffset? LastLoginAt);
