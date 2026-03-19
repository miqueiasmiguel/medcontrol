using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Commands.RemovePaymentItem;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using NSubstitute;

namespace MedControl.Application.Tests.Payments;

public sealed class RemovePaymentItemCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly RemovePaymentItemCommandHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public RemovePaymentItemCommandHandlerTests()
    {
        _sut = new RemovePaymentItemCommandHandler(_paymentRepository, _unitOfWork, _currentTenant);
    }

    private static Payment CreatePaymentWithItems(Guid tenantId, int itemCount)
    {
        var items = Enumerable.Range(0, itemCount)
            .Select(_ => (Guid.NewGuid(), 100m + _ * 50m))
            .ToArray();
        return Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Today,
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;
    }

    [Fact]
    public async Task Handle_Com2Itens_DeveRemoverESalvar()
    {
        var tenantId = Guid.NewGuid();
        var payment = CreatePaymentWithItems(tenantId, 2);
        var itemId = payment.Items[0].Id;
        _currentTenant.TenantId.Returns(tenantId);
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(new RemovePaymentItemCommand(payment.Id, itemId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        await _paymentRepository.Received(1).UpdateAsync(payment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UltimoItem_DeveRetornarValidationErrENaoAtualizar()
    {
        var tenantId = Guid.NewGuid();
        var payment = CreatePaymentWithItems(tenantId, 1);
        var itemId = payment.Items[0].Id;
        _currentTenant.TenantId.Returns(tenantId);
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _sut.Handle(new RemovePaymentItemCommand(payment.Id, itemId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        await _paymentRepository.DidNotReceive().UpdateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentNaoEncontrado_DeveRetornarNotFound()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _paymentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _sut.Handle(new RemovePaymentItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new RemovePaymentItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
