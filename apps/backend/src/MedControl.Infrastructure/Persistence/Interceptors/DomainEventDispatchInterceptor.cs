using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MedControl.Infrastructure.Persistence.Interceptors;

public sealed class DomainEventDispatchInterceptor(
    IServiceProvider serviceProvider,
    ILogger<DomainEventDispatchInterceptor> logger) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in events)
        {
            await DispatchEventAsync(domainEvent, ct);
        }
    }

    private async Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            return;
        }

        logger.LogDebug(
            "Dispatching domain event {EventType} to {HandlerCount} handler(s)",
            eventType.Name,
            handlers.Count);

        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

        foreach (var handler in handlers)
        {
            await (Task)handleMethod.Invoke(handler, [domainEvent, ct])!;
        }
    }
}
