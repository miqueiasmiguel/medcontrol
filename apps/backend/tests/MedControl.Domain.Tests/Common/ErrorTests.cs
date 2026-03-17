using FluentAssertions;
using MedControl.Domain.Common;

namespace MedControl.Domain.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Create_WithFactoryMethod_ShouldStoreValues()
    {
        var error = Error.Failure("User.NotFound", "User was not found.");

        error.Code.Should().Be("User.NotFound");
        error.Description.Should().Be("User was not found.");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void None_ShouldHaveEmptyCodeAndDescriptionAndTypeNone()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Description.Should().BeEmpty();
        Error.None.Type.Should().Be(ErrorType.None);
    }

    [Fact]
    public void NullValue_ShouldHaveExpectedCodeAndTypeFailure()
    {
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Description.Should().NotBeEmpty();
        Error.NullValue.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void TwoErrors_WithSameCodeDescriptionAndType_ShouldBeEqual()
    {
        var a = Error.Failure("X.Code", "desc");
        var b = Error.Failure("X.Code", "desc");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoErrors_WithSameCodeAndDescriptionButDifferentType_ShouldNotBeEqual()
    {
        var a = Error.Failure("X.Code", "desc");
        var b = Error.Validation("X.Code", "desc");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Validation_ShouldHaveTypeValidation()
    {
        var error = Error.Validation("X.Code", "desc");

        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NotFound_ShouldHaveTypeNotFound()
    {
        var error = Error.NotFound("X.Code", "desc");

        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Conflict_ShouldHaveTypeConflict()
    {
        var error = Error.Conflict("X.Code", "desc");

        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Unauthorized_ShouldHaveTypeUnauthorized()
    {
        var error = Error.Unauthorized("X.Code", "desc");

        error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldHaveTypeForbidden()
    {
        var error = Error.Forbidden("X.Code", "desc");

        error.Type.Should().Be(ErrorType.Forbidden);
    }
}
