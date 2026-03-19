using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using NSubstitute;

namespace MedControl.Application.Tests.Payments;

public sealed class CreatePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly CreatePaymentCommandHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public CreatePaymentCommandHandlerTests()
    {
        _sut = new CreatePaymentCommandHandler(_paymentRepository, _unitOfWork, _currentTenant);
    }

    private static CreatePaymentCommand BuildCommand(IReadOnlyList<CreatePaymentItemRequest>? items = null) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Today,
            "ATD-001",
            null,
            "123456789",
            "João Silva",
            "Hospital ABC",
            "Convênio XYZ",
            null,
            items ?? [new CreatePaymentItemRequest(Guid.NewGuid(), 150.00m)]);

    [Fact]
    public async Task Handle_ComDadosValidos_DeveSalvarERetornarDto()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);

        var command = BuildCommand();
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.AppointmentNumber.Should().Be("ATD-001");
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be("Pending");
        await _paymentRepository.Received(1).AddAsync(
            Arg.Is<Payment>(p =>
                p.TenantId == tenantId &&
                p.AppointmentNumber == "ATD-001" &&
                p.Items.Count == 1),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        await _paymentRepository.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validator_ComAppointmentNumberVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreatePaymentCommandValidator();
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Today,
            string.Empty, null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null,
            [new CreatePaymentItemRequest(Guid.NewGuid(), 150.00m)]);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComItemsVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreatePaymentCommandValidator();
        var command = BuildCommand([]);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComItemValueZero_DeveRetornarErroDeValidacao()
    {
        var validator = new CreatePaymentCommandValidator();
        var command = BuildCommand([new CreatePaymentItemRequest(Guid.NewGuid(), 0m)]);

        var validation = await validator.ValidateAsync(command);
        validation.IsValid.Should().BeFalse();
    }
}
