using FluentAssertions;
using MedControl.Domain.Common;

namespace MedControl.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldBeSuccessAndHaveNoError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldNotBeSuccessAndShouldContainError()
    {
        var error = Error.Failure("Domain.Error", "Something went wrong.");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithNoneError_ShouldThrowInvalidOperationException()
    {
        var act = () => Result.Failure(Error.None);

        act.Should().Throw<InvalidOperationException>();
    }
}

public class ResultTTests
{
    [Fact]
    public void Success_ShouldBeSuccessAndReturnValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldNotBeSuccessAndContainError()
    {
        var error = Error.Failure("Domain.Error", "Something went wrong.");

        var result = Result.Failure<int>(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_WhenFailure_ShouldThrowInvalidOperationException()
    {
        var result = Result.Failure<int>(Error.Failure("X", "desc"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }
}
