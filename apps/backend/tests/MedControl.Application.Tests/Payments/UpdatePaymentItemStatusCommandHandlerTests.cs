using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Commands.UpdatePaymentItemStatus;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Payments;

public sealed class UpdatePaymentItemStatusCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly UpdatePaymentItemStatusCommandHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public UpdatePaymentItemStatusCommandHandlerTests()
    {
        _sut = new UpdatePaymentItemStatusCommandHandler(_paymentRepository, _unitOfWork, _currentTenant);
    }

    private static Payment BuildPayment(Guid tenantId)
    {
        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        return Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;
    }

    [Fact]
    public async Task Handle_QuandoItemExiste_DeveAtualizarStatus()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var payment = BuildPayment(tenantId);
        var itemId = payment.Items[0].Id;

        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var command = new UpdatePaymentItemStatusCommand(payment.Id, itemId, PaymentStatus.Paid, "Pago");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].Status.Should().Be("Paid");
        result.Value.Items[0].Notes.Should().Be("Pago");
        await _paymentRepository.Received(1).UpdateAsync(payment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoPaymentNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _paymentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var command = new UpdatePaymentItemStatusCommand(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Paid, null);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _paymentRepository.DidNotReceive().UpdateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoItemNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var payment = BuildPayment(tenantId);

        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var command = new UpdatePaymentItemStatusCommand(payment.Id, Guid.NewGuid(), PaymentStatus.Paid, null);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new UpdatePaymentItemStatusCommand(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Paid, null);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_ComStatusInvalido_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdatePaymentItemStatusCommandValidator();
        var command = new UpdatePaymentItemStatusCommand(Guid.NewGuid(), Guid.NewGuid(), (PaymentStatus)99, null);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }
}
