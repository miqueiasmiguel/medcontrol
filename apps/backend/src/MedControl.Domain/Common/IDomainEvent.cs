namespace MedControl.Domain.Common;

public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}
