using FluentAssertions;
using MedControl.Application.Payments.Queries.ListPayments;
using MedControl.Domain.Payments;
using NSubstitute;

namespace MedControl.Application.Tests.Payments;

public sealed class ListPaymentsQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly ListPaymentsQueryHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public ListPaymentsQueryHandlerTests()
    {
        _sut = new ListPaymentsQueryHandler(_paymentRepository);
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

        _paymentRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Payment> { payment });

        var result = await _sut.Handle(new ListPaymentsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].AppointmentNumber.Should().Be("ATD-001");
        result.Value[0].Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ComListaVazia_DeveRetornarListaVazia()
    {
        _paymentRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        var result = await _sut.Handle(new ListPaymentsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
