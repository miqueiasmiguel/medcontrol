using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.HealthPlans;
using MedControl.Domain.Tenants.Events;

namespace MedControl.Application.Tenants.Events;

public sealed class SeedDefaultHealthPlansOnTenantCreatedHandler(
    IHealthPlanRepository healthPlanRepository,
    IUnitOfWork unitOfWork)
    : IDomainEventHandler<TenantCreatedEvent>
{
    private static readonly (string Name, string TissCode)[] DefaultHealthPlans =
    [
        // Nacionais
        ("Hapvida", "368253"),
        ("NotreDame Intermédica", "359017"),
        ("Bradesco Saúde", "421715"),
        ("Amil", "326305"),
        ("SulAmérica Saúde", "006246"),
        ("Central Nacional Unimed", "339679"),
        ("GEAP Saúde", "323080"),
        ("Golden Cross", "005762"),
        ("Cassi", "302147"),
        ("Postal Saúde", "325031"),
        ("Porto Seguro Saúde", "327689"),
        // Paraíba
        ("Unimed João Pessoa", "320080"),
        ("Unimed Campina Grande", "313467"),
        ("Unimed Sousa", "303763"),
        ("Unimed Patos", "305936"),
        ("Unimed Fortaleza", "317144"),
    ];

    public async Task Handle(TenantCreatedEvent domainEvent, CancellationToken ct)
    {
        foreach (var (name, tissCode) in DefaultHealthPlans)
        {
            var result = HealthPlan.Create(domainEvent.AggregateId, name, tissCode);
            if (result.IsSuccess)
            {
                await healthPlanRepository.AddAsync(result.Value, ct);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
