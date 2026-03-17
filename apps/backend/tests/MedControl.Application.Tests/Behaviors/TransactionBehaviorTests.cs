using FluentAssertions;
using MedControl.Application.Behaviors;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace MedControl.Application.Tests.Behaviors;

public sealed class TransactionBehaviorTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenQuery_ShouldSkipTransaction()
    {
        var sut = new TransactionBehavior<TestQuery, string>(
            _unitOfWork,
            NullLogger<TransactionBehavior<TestQuery, string>>.Instance);
        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        var result = await sut.Handle(new TestQuery(), next, CancellationToken.None);

        result.Should().Be("result");
        await _unitOfWork.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommand_ShouldBeginAndCommitTransaction()
    {
        var sut = new TransactionBehavior<TestCommand, Unit>(
            _unitOfWork,
            NullLogger<TransactionBehavior<TestCommand, Unit>>.Instance);
        RequestHandlerDelegate<Unit> next = () => Task.FromResult(Unit.Value);

        await sut.Handle(new TestCommand(), next, CancellationToken.None);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandThrows_ShouldRollbackAndRethrow()
    {
        var sut = new TransactionBehavior<TestCommand, Unit>(
            _unitOfWork,
            NullLogger<TransactionBehavior<TestCommand, Unit>>.Instance);
        RequestHandlerDelegate<Unit> next = () => throw new InvalidOperationException("handler error");

        var act = () => sut.Handle(new TestCommand(), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    public sealed record TestCommand : ICommand;
    public sealed record TestQuery : IQuery<string>;
}
