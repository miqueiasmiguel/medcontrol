using MedControl.Domain.Common;

namespace MedControl.Application.Mediator;

public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken ct);
}
