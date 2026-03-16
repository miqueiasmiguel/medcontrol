using MedControl.Domain.Common;
using MedControl.Domain.Users.Events;

namespace MedControl.Domain.Users;

public sealed class User : BaseAuditableEntity, IAggregateRoot
{
    private User() { } // EF Core

    public string Email { get; private set; } = default!;
    public string? DisplayName { get; private set; }
    public Uri? AvatarUrl { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public GlobalRole GlobalRole { get; private set; } = GlobalRole.None;
    public DateTimeOffset? LastLoginAt { get; private set; }

    public static User Create(string email, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var user = new User
        {
#pragma warning disable CA1308 // emails must be stored lowercase by convention
            Email = email.Trim().ToLowerInvariant(),
#pragma warning restore CA1308
            DisplayName = displayName?.Trim(),
            IsEmailVerified = false,
        };

        user.Raise(new UserRegisteredEvent(user.Id, user.Email, DateTimeOffset.UtcNow));
        return user;
    }

    /// <summary>
    /// Creates or updates a user from Google OAuth profile.
    /// Email is always verified when coming from Google.
    /// </summary>
    public static User CreateFromGoogle(string email, string displayName, Uri? avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var user = new User
        {
#pragma warning disable CA1308 // emails must be stored lowercase by convention
            Email = email.Trim().ToLowerInvariant(),
#pragma warning restore CA1308
            DisplayName = displayName.Trim(),
            AvatarUrl = avatarUrl,
            IsEmailVerified = true,
        };

        user.Raise(new UserRegisteredEvent(user.Id, user.Email, DateTimeOffset.UtcNow));
        return user;
    }

    public void UpdateProfile(string? displayName, Uri? avatarUrl)
    {
        DisplayName = displayName?.Trim();
        AvatarUrl = avatarUrl;
    }

    public void VerifyEmail() => IsEmailVerified = true;

    public void RecordLogin() => LastLoginAt = DateTimeOffset.UtcNow;

    public void SetGlobalRole(GlobalRole role) => GlobalRole = role;

    public bool IsGlobalAdmin() => GlobalRole == GlobalRole.Admin;
    public bool IsGlobalSupport() => GlobalRole is GlobalRole.Admin or GlobalRole.Support;
}
