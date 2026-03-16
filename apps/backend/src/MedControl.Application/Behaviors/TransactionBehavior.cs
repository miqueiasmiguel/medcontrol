using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using Microsoft.Extensions.Logging;

namespace MedControl.Application.Behaviors;

/// <summary>
/// Wraps ICommand handlers in a database transaction.
/// IQuery handlers are skipped (read-only, no transaction needed).
/// </summary>
internal sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Only wrap Commands in a transaction, not Queries
        if (request is not ICommand and not ICommand<TResponse>)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        logger.LogDebug("Beginning transaction for {RequestName}", requestName);

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var response = await next();
            await unitOfWork.CommitTransactionAsync(ct);
            logger.LogDebug("Committed transaction for {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rolling back transaction for {RequestName}", requestName);
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
