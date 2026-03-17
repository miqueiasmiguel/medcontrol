using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Users;
using MedControl.Domain.Users.Events;

namespace MedControl.Domain.Tests.Users;

public class UserCreateTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldReturnFailure(string? email)
    {
        var result = User.Create(email!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(User.Errors.EmailRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithValidEmail_ShouldReturnSuccessWithUser()
    {
        var result = User.Create("User@Example.COM");

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@example.com");
        result.Value.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidEmail_ShouldRaiseUserRegisteredEvent()
    {
        var result = User.Create("user@test.com");

        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredEvent>();
    }
}

public class UserCreateFromGoogleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFromGoogle_WithInvalidEmail_ShouldReturnFailure(string? email)
    {
        var result = User.CreateFromGoogle(email!, "João", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(User.Errors.EmailRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFromGoogle_WithInvalidDisplayName_ShouldReturnFailure(string? displayName)
    {
        var result = User.CreateFromGoogle("user@test.com", displayName!, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(User.Errors.DisplayNameRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreateFromGoogle_WithValidData_ShouldReturnSuccessWithVerifiedUser()
    {
        var avatar = new Uri("https://example.com/avatar.png");

        var result = User.CreateFromGoogle("User@Google.COM", "  João Silva  ", avatar);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@google.com");
        result.Value.DisplayName.Should().Be("João Silva");
        result.Value.AvatarUrl.Should().Be(avatar);
        result.Value.IsEmailVerified.Should().BeTrue();
    }
}
