using FluentAssertions;
using FluentValidation;
using MedControl.Application.Behaviors;
using MedControl.Application.Mediator;

namespace MedControl.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        var sut = new ValidationBehavior<TestRequest, string>([]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var result = await sut.Handle(new TestRequest("valid"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenAllValidatorsPass_ShouldCallNext()
    {
        var sut = new ValidationBehavior<TestRequest, string>([new PassingValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var result = await sut.Handle(new TestRequest("valid"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ShouldThrowValidationException()
    {
        var sut = new ValidationBehavior<TestRequest, string>([new FailingValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var act = () => sut.Handle(new TestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsAndOneFails_ShouldThrowValidationException()
    {
        var sut = new ValidationBehavior<TestRequest, string>([new PassingValidator(), new FailingValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var act = () => sut.Handle(new TestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    public sealed record TestRequest(string Value) : IRequest<string>;

    private sealed class PassingValidator : AbstractValidator<TestRequest>
    {
        public PassingValidator() => RuleFor(x => x.Value).NotNull();
    }

    private sealed class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator() => RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required.");
    }
}
