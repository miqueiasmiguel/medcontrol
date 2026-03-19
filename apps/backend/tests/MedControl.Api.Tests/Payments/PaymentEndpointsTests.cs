using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedControl.Api.Tests.Helpers;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Payments;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Api.Tests.Payments;

public sealed class PaymentEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public PaymentEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // GET /payments

    [Fact]
    public async Task GET_payments_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/payments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_payments_Autenticado_ComTenant_Retorna200ComLista()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.PaymentRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        var response = await client.GetAsync("/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();
        body.Should().BeEmpty();
    }

    // GET /payments/{id}

    [Fact]
    public async Task GET_payments_id_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync($"/payments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_payments_id_NaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.PaymentRepository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var response = await client.GetAsync($"/payments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_payments_id_Encontrado_Retorna200()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;

        _factory.PaymentRepository
            .GetByIdAsync(payment.Id, Arg.Any<CancellationToken>())
            .Returns(payment);

        var response = await client.GetAsync($"/payments/{payment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentDto>();
        body.Should().NotBeNull();
        body!.AppointmentNumber.Should().Be("ATD-001");
    }

    // POST /payments

    [Fact]
    public async Task POST_payments_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PostAsJsonAsync("/payments", new
        {
            doctorId = Guid.NewGuid(),
            healthPlanId = Guid.NewGuid(),
            executionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            appointmentNumber = "ATD-001",
            beneficiaryCard = "123456789",
            beneficiaryName = "João Silva",
            executionLocation = "Hospital ABC",
            paymentLocation = "Convênio XYZ",
            items = new[] { new { procedureId = Guid.NewGuid(), value = 150.00m } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_payments_CamposInvalidos_Retorna400()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var response = await client.PostAsJsonAsync("/payments", new
        {
            doctorId = Guid.NewGuid(),
            healthPlanId = Guid.NewGuid(),
            executionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            appointmentNumber = string.Empty,
            beneficiaryCard = string.Empty,
            beneficiaryName = string.Empty,
            executionLocation = string.Empty,
            paymentLocation = string.Empty,
            items = Array.Empty<object>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_payments_DadosValidos_Retorna201ComPaymentDto()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var response = await client.PostAsJsonAsync("/payments", new
        {
            doctorId = Guid.NewGuid(),
            healthPlanId = Guid.NewGuid(),
            executionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            appointmentNumber = "ATD-001",
            beneficiaryCard = "123456789",
            beneficiaryName = "João Silva",
            executionLocation = "Hospital ABC",
            paymentLocation = "Convênio XYZ",
            items = new[] { new { procedureId = Guid.NewGuid(), value = 150.00m } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PaymentDto>();
        body.Should().NotBeNull();
        body!.AppointmentNumber.Should().Be("ATD-001");
        body.Items.Should().HaveCount(1);
        body.Items[0].Status.Should().Be("Pending");
    }

    // PATCH /payments/{paymentId}/items/{itemId}

    [Fact]
    public async Task PATCH_payments_items_SemAutenticacao_Retorna401()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var response = await client.PatchAsJsonAsync(
            $"/payments/{Guid.NewGuid()}/items/{Guid.NewGuid()}",
            new { status = "Paid" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PATCH_payments_items_PaymentNaoEncontrado_Retorna404()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        _factory.PaymentRepository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var response = await client.PatchAsJsonAsync(
            $"/payments/{Guid.NewGuid()}/items/{Guid.NewGuid()}",
            new { status = PaymentStatus.Paid });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_payments_items_DadosValidos_Retorna200()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId, "user@example.com", tenantId);

        var items = new List<(Guid ProcedureId, decimal Value)> { (Guid.NewGuid(), 150.00m) };
        var payment = Payment.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "ATD-001", null, "123456789", "João Silva",
            "Hospital ABC", "Convênio XYZ", null, items).Value;
        var itemId = payment.Items[0].Id;

        _factory.PaymentRepository
            .GetByIdAsync(payment.Id, Arg.Any<CancellationToken>())
            .Returns(payment);

        var response = await client.PatchAsJsonAsync(
            $"/payments/{payment.Id}/items/{itemId}",
            new { status = PaymentStatus.Paid, notes = "Pago" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentDto>();
        body.Should().NotBeNull();
        body!.Items[0].Status.Should().Be("Paid");
    }
}
