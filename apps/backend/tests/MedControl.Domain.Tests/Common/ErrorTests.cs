using FluentAssertions;
using MedControl.Domain.Common;

namespace MedControl.Domain.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Create_WithCodeAndDescription_ShouldStoreValues()
    {
        var error = new Error("User.NotFound", "User was not found.");

        error.Code.Should().Be("User.NotFound");
        error.Description.Should().Be("User was not found.");
    }

    [Fact]
    public void None_ShouldHaveEmptyCodeAndDescription()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Description.Should().BeEmpty();
    }

    [Fact]
    public void NullValue_ShouldHaveExpectedCode()
    {
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void TwoErrors_WithSameCodeAndDescription_ShouldBeEqual()
    {
        var a = new Error("X.Code", "desc");
        var b = new Error("X.Code", "desc");

        a.Should().Be(b);
    }
}
