using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Commands.AddPaymentItem;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using NSubstitute;

namespace MedControl.Application.Tests.Payments;

public sealed class AddPaymentItemCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly AddPaymentItemCommandHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public AddPaymentItemCommandHandlerTests()
    {
        _sut = new AddPaymentItemCommandHandler(_paymentRepository, _unitOfWork, _currentTenant);
    }

    private static Payment CreatePayment(Guid tenantId) =>
        Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null,
            [(Guid.NewGuid(), 150m)]).Value;

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAdicionarItemESalvar()
    {
        var tenantId = Guid.NewGuid();
        var payment = CreatePayment(tenantId);
        _currentTenant.TenantId.Returns(tenantId);
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var command = new AddPaymentItemCommand(payment.Id, Guid.NewGuid(), 200m);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        await _paymentRepository.Received(1).UpdateAsync(payment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentNaoEncontrado_DeveRetornarNotFound()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _paymentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _sut.Handle(new AddPaymentItemCommand(Guid.NewGuid(), Guid.NewGuid(), 100m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _paymentRepository.DidNotReceive().UpdateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new AddPaymentItemCommand(Guid.NewGuid(), Guid.NewGuid(), 100m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_ComProcedureIdVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new AddPaymentItemCommandValidator();
        var command = new AddPaymentItemCommand(Guid.NewGuid(), Guid.Empty, 100m);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComValueZero_DeveRetornarErroDeValidacao()
    {
        var validator = new AddPaymentItemCommandValidator();
        var command = new AddPaymentItemCommand(Guid.NewGuid(), Guid.NewGuid(), 0m);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }
}
