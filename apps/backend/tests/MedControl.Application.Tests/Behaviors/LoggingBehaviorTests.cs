using FluentAssertions;
using MedControl.Application.Behaviors;
using MedControl.Application.Mediator;
using Microsoft.Extensions.Logging.Abstractions;

namespace MedControl.Application.Tests.Behaviors;

public sealed class LoggingBehaviorTests
{
    private readonly LoggingBehavior<TestRequest, string> _sut =
        new(NullLogger<LoggingBehavior<TestRequest, string>>.Instance);

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldReturnNextResult()
    {
        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        var result = await _sut.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be("result");
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldRethrow()
    {
        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("boom");

        var act = () => _sut.Handle(new TestRequest(), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    public sealed record TestRequest : IRequest<string>;
}
