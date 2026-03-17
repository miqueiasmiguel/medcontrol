using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace MedControl.Application.Tests.Mediator;

public sealed class MediatorTests
{
    private static IMediator BuildMediator(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IUnitOfWork>(Substitute.For<IUnitOfWork>());
        services.AddMediator(typeof(MediatorTests).Assembly);
        configure?.Invoke(services);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_WithRegisteredHandler_ShouldReturnResult()
    {
        var mediator = BuildMediator();

        var result = await mediator.Send(new EchoQuery("hello"));

        result.Should().Be("hello");
    }

    [Fact]
    public async Task Send_WithCommand_ShouldExecuteHandler()
    {
        var mediator = BuildMediator();

        var result = await mediator.Send(new NoOpCommand());

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Send_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var mediator = BuildMediator();

        var act = () => mediator.Send<string>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Send_WithoutRegisteredHandler_ShouldThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, MedControl.Application.Mediator.Mediator>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var act = () => mediator.Send(new EchoQuery("hello"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Test doubles ─────────────────────────────────────────────────────────

    public sealed record EchoQuery(string Value) : IQuery<string>;
    public sealed record NoOpCommand : ICommand;

    public sealed class EchoQueryHandler : IRequestHandler<EchoQuery, string>
    {
        public Task<string> Handle(EchoQuery request, CancellationToken ct) =>
            Task.FromResult(request.Value);
    }

    public sealed class NoOpCommandHandler : IRequestHandler<NoOpCommand, Unit>
    {
        public Task<Unit> Handle(NoOpCommand request, CancellationToken ct) =>
            Task.FromResult(Unit.Value);
    }
}
