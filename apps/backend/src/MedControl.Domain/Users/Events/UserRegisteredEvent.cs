using MedControl.Domain.Common;

namespace MedControl.Domain.Users.Events;

public sealed record UserRegisteredEvent(
    Guid AggregateId,
    string Email,
    DateTimeOffset OccurredAt) : IDomainEvent;
