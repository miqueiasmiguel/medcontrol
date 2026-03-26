using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Queries.GetPayment;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Payments;

public sealed class GetPaymentQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly GetPaymentQueryHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public GetPaymentQueryHandlerTests()
    {
        _currentUser.Roles.Returns(new List<string>());
        _sut = new GetPaymentQueryHandler(_paymentRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_QuandoExiste_DeveRetornarDto()
    {
        var tenantId = Guid.NewGuid();
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(new GetPaymentQuery(payment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(payment.Id);
        result.Value.AppointmentNumber.Should().Be("ATD-001");
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _paymentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(new GetPaymentQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_QuandoRoleDoctor_EDoctorIdCorresponde_DeveRetornarDto()
    {
        var doctorUserId = Guid.NewGuid();
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), doctorUserId, Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        _currentUser.Roles.Returns(new List<string> { "doctor" });
        _currentUser.UserId.Returns(doctorUserId);
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(new GetPaymentQuery(payment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task Handle_QuandoRoleDoctor_EDoctorIdNaoCorresponde_DeveRetornarUnauthorized()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        _currentUser.Roles.Returns(new List<string> { "doctor" });
        _currentUser.UserId.Returns(Guid.NewGuid()); // diferente do payment.DoctorId

        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(new GetPaymentQuery(payment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_QuandoRoleOperador_DeveRetornarDto_SemChecarDoctorId()
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        _currentUser.Roles.Returns(new List<string> { "operator" });
        _currentUser.UserId.Returns(Guid.NewGuid()); // diferente do payment.DoctorId — não importa para operadores

        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(new GetPaymentQuery(payment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
