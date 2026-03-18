using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Tenants.Events;
using MedControl.Domain.HealthPlans;
using MedControl.Domain.Tenants.Events;
using NSubstitute;

namespace MedControl.Application.Tests.Tenants;

public sealed class SeedDefaultHealthPlansOnTenantCreatedHandlerTests
{
    private readonly IHealthPlanRepository _healthPlanRepository = Substitute.For<IHealthPlanRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SeedDefaultHealthPlansOnTenantCreatedHandler _sut;

    public SeedDefaultHealthPlansOnTenantCreatedHandlerTests()
    {
        _sut = new SeedDefaultHealthPlansOnTenantCreatedHandler(_healthPlanRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_QuandoTenantCriado_DeveCadastrar8ConveniosPadrao()
    {
        var tenantId = Guid.NewGuid();
        var domainEvent = new TenantCreatedEvent(tenantId, "Clínica Teste", DateTimeOffset.UtcNow);

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _healthPlanRepository.Received(8).AddAsync(
            Arg.Any<HealthPlan>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoTenantCriado_DeveVincularConveniosAoTenantCorreto()
    {
        var tenantId = Guid.NewGuid();
        var domainEvent = new TenantCreatedEvent(tenantId, "Clínica Teste", DateTimeOffset.UtcNow);

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _healthPlanRepository.Received().AddAsync(
            Arg.Is<HealthPlan>(hp => hp.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoTenantCriado_DeveIncluirHapvida()
    {
        var tenantId = Guid.NewGuid();
        var domainEvent = new TenantCreatedEvent(tenantId, "Clínica Teste", DateTimeOffset.UtcNow);

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _healthPlanRepository.Received(1).AddAsync(
            Arg.Is<HealthPlan>(hp => hp.TissCode == "368253" && hp.Name == "Hapvida"),
            Arg.Any<CancellationToken>());
    }
}
