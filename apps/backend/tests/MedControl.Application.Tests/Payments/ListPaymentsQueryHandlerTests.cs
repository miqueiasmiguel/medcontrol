using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Queries.ListPayments;
using MedControl.Domain.Payments;
using NSubstitute;

namespace MedControl.Application.Tests.Payments;

public sealed class ListPaymentsQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ListPaymentsQueryHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public ListPaymentsQueryHandlerTests()
    {
        _currentUser.Roles.Returns(new List<string>());
        _sut = new ListPaymentsQueryHandler(_paymentRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_DeveRetornarListaDeDtos()
    {
        var tenantId = Guid.NewGuid();
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 100.00m) };
        var payment = Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        _paymentRepository.ListAsync(Arg.Any<PaymentFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment> { payment });

        var result = await _sut.Handle(new ListPaymentsQuery(new PaymentFilters()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].AppointmentNumber.Should().Be("ATD-001");
        result.Value[0].Items.Should().HaveCount(1);
        result.Value[0].TotalValue.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_ComListaVazia_DeveRetornarListaVazia()
    {
        _paymentRepository.ListAsync(Arg.Any<PaymentFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        var result = await _sut.Handle(new ListPaymentsQuery(new PaymentFilters()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_QuandoRoleDoctor_DeveForcaDoctorIdParaUsuarioAtual()
    {
        var doctorUserId = Guid.NewGuid();
        _currentUser.Roles.Returns(new List<string> { "doctor" });
        _currentUser.UserId.Returns(doctorUserId);

        _paymentRepository.ListAsync(Arg.Any<PaymentFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        await _sut.Handle(new ListPaymentsQuery(new PaymentFilters()), CancellationToken.None);

        await _paymentRepository.Received(1).ListAsync(
            Arg.Is<PaymentFilters>(f => f.DoctorId == doctorUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoRoleDoctor_IgnoraDoctoridPassadoNaQuery()
    {
        var doctorUserId = Guid.NewGuid();
        var outroMedicoId = Guid.NewGuid();
        _currentUser.Roles.Returns(new List<string> { "doctor" });
        _currentUser.UserId.Returns(doctorUserId);

        _paymentRepository.ListAsync(Arg.Any<PaymentFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        await _sut.Handle(new ListPaymentsQuery(new PaymentFilters(DoctorId: outroMedicoId)), CancellationToken.None);

        await _paymentRepository.Received(1).ListAsync(
            Arg.Is<PaymentFilters>(f => f.DoctorId == doctorUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoRoleOperador_NaoForcaDoctorId()
    {
        _currentUser.Roles.Returns(new List<string> { "operator" });
        _currentUser.UserId.Returns(Guid.NewGuid());

        _paymentRepository.ListAsync(Arg.Any<PaymentFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        var filters = new PaymentFilters();
        await _sut.Handle(new ListPaymentsQuery(filters), CancellationToken.None);

        await _paymentRepository.Received(1).ListAsync(
            Arg.Is<PaymentFilters>(f => f.DoctorId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassaFiltrosParaRepositorio()
    {
        var healthPlanId = Guid.NewGuid();
        var dateFrom = new DateOnly(2026, 1, 1);
        var dateTo = new DateOnly(2026, 3, 31);
        var filters = new PaymentFilters(
            HealthPlanId: healthPlanId,
            Status: PaymentStatus.Paid,
            DateFrom: dateFrom,
            DateTo: dateTo,
            Search: "João");

        _paymentRepository.ListAsync(Arg.Any<PaymentFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        await _sut.Handle(new ListPaymentsQuery(filters), CancellationToken.None);

        await _paymentRepository.Received(1).ListAsync(
            Arg.Is<PaymentFilters>(f =>
                f.HealthPlanId == healthPlanId &&
                f.Status == PaymentStatus.Paid &&
                f.DateFrom == dateFrom &&
                f.DateTo == dateTo &&
                f.Search == "João"),
            Arg.Any<CancellationToken>());
    }
}
