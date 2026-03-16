using MedControl.Domain.Common;

namespace MedControl.Domain.Tenants.Events;

public sealed record TenantCreatedEvent(
    Guid AggregateId,
    string TenantName,
    DateTimeOffset OccurredAt) : IDomainEvent;
