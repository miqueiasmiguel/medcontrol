using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Commands.UpdatePayment;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using NSubstitute;

namespace MedControl.Application.Tests.Payments;

public sealed class UpdatePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly UpdatePaymentCommandHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public UpdatePaymentCommandHandlerTests()
    {
        _sut = new UpdatePaymentCommandHandler(_paymentRepository, _unitOfWork, _currentTenant);
    }

    private static Payment CreatePayment(Guid tenantId) =>
        Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null,
            [(Guid.NewGuid(), 150m)]).Value;

    private static UpdatePaymentCommand BuildCommand(Guid paymentId) =>
        new(paymentId, Today, "ATD-002", null, "987654321",
            "Maria Santos", "Hospital XYZ", "Convênio ABC", null);

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAtualizarESalvar()
    {
        var tenantId = Guid.NewGuid();
        var payment = CreatePayment(tenantId);
        _currentTenant.TenantId.Returns(tenantId);
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(BuildCommand(payment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AppointmentNumber.Should().Be("ATD-002");
        await _paymentRepository.Received(1).UpdateAsync(payment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentNaoEncontrado_DeveRetornarNotFound()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _paymentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _sut.Handle(BuildCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _paymentRepository.DidNotReceive().UpdateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(BuildCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_ComAppointmentNumberVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdatePaymentCommandValidator();
        var command = new UpdatePaymentCommand(Guid.NewGuid(), Today, string.Empty, null,
            "123456789", "João Silva", "Hospital ABC", "Convênio XYZ", null);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComBeneficiaryCardVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdatePaymentCommandValidator();
        var command = new UpdatePaymentCommand(Guid.NewGuid(), Today, "ATD-001", null,
            string.Empty, "João Silva", "Hospital ABC", "Convênio XYZ", null);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }
}
