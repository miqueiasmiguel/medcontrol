using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.AddPaymentItem;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.Commands.RemovePaymentItem;
using MedControl.Application.Payments.Commands.UpdatePayment;
using MedControl.Application.Payments.Commands.UpdatePaymentItemStatus;
using MedControl.Application.Payments.DTOs;
using MedControl.Application.Payments.Queries.GetPayment;
using MedControl.Application.Payments.Queries.ListPayments;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;
using Microsoft.AspNetCore.Mvc;

namespace MedControl.Api.Endpoints.Payments;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPayments(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListPayments)
            .WithName("ListPayments")
            .RequireAuthorization()
            .Produces<IReadOnlyList<PaymentDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetPayment)
            .WithName("GetPayment")
            .RequireAuthorization()
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .RequireAuthorization()
            .Produces<PaymentDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{paymentId:guid}/items/{itemId:guid}", UpdatePaymentItemStatus)
            .WithName("UpdatePaymentItemStatus")
            .RequireAuthorization()
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}", UpdatePayment)
            .WithName("UpdatePayment")
            .RequireAuthorization()
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{paymentId:guid}/items", AddPaymentItem)
            .WithName("AddPaymentItem")
            .RequireAuthorization()
            .Produces<PaymentDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{paymentId:guid}/items/{itemId:guid}", RemovePaymentItem)
            .WithName("RemovePaymentItem")
            .RequireAuthorization()
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ListPayments(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? healthPlanId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool? sortDescending,
        IMediator mediator,
        CancellationToken ct)
    {
        var statusEnum = Enum.TryParse<PaymentStatus>(status, ignoreCase: true, out var s) ? s : (PaymentStatus?)null;
        var sortByEnum = Enum.TryParse<PaymentSortBy>(sortBy, ignoreCase: true, out var sb) ? sb : PaymentSortBy.ExecutionDate;

        var filters = new PaymentFilters(
            doctorId,
            healthPlanId,
            statusEnum,
            dateFrom,
            dateTo,
            search,
            sortByEnum,
            sortDescending ?? true);

        var result = await mediator.Send<Result<IReadOnlyList<PaymentDto>>>(new ListPaymentsQuery(filters), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result.Error);
    }

    private static async Task<IResult> GetPayment(
        [FromRoute] Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<PaymentDto>>(new GetPaymentQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result.Error);
    }

    private static async Task<IResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        ICurrentUserService currentUser,
        IMediator mediator,
        CancellationToken ct)
    {
        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase))
        {
            return Results.Problem("Doctors cannot modify payments.", statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await mediator.Send<Result<PaymentDto>>(
            new CreatePaymentCommand(
                request.DoctorId,
                request.HealthPlanId,
                request.ExecutionDate,
                request.AppointmentNumber,
                request.AuthorizationCode,
                request.BeneficiaryCard,
                request.BeneficiaryName,
                request.ExecutionLocation,
                request.PaymentLocation,
                request.Notes,
                request.Items), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Created($"/payments/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdatePaymentItemStatus(
        [FromRoute] Guid paymentId,
        [FromRoute] Guid itemId,
        [FromBody] UpdatePaymentItemStatusRequest request,
        ICurrentUserService currentUser,
        IMediator mediator,
        CancellationToken ct)
    {
        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase))
        {
            return Results.Problem("Doctors cannot modify payments.", statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await mediator.Send<Result<PaymentDto>>(
            new UpdatePaymentItemStatusCommand(paymentId, itemId, request.Status, request.Notes), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result.Error);
    }

    private static async Task<IResult> UpdatePayment(
        [FromRoute] Guid id,
        [FromBody] UpdatePaymentRequest request,
        ICurrentUserService currentUser,
        IMediator mediator,
        CancellationToken ct)
    {
        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase))
        {
            return Results.Problem("Doctors cannot modify payments.", statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await mediator.Send<Result<PaymentDto>>(
            new UpdatePaymentCommand(
                id,
                request.ExecutionDate,
                request.AppointmentNumber,
                request.AuthorizationCode,
                request.BeneficiaryCard,
                request.BeneficiaryName,
                request.ExecutionLocation,
                request.PaymentLocation,
                request.Notes), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result.Error);
    }

    private static async Task<IResult> AddPaymentItem(
        [FromRoute] Guid paymentId,
        [FromBody] AddPaymentItemRequest request,
        ICurrentUserService currentUser,
        IMediator mediator,
        CancellationToken ct)
    {
        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase))
        {
            return Results.Problem("Doctors cannot modify payments.", statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await mediator.Send<Result<PaymentDto>>(
            new AddPaymentItemCommand(paymentId, request.ProcedureId, request.Value), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Created($"/payments/{paymentId}", result.Value);
    }

    private static async Task<IResult> RemovePaymentItem(
        [FromRoute] Guid paymentId,
        [FromRoute] Guid itemId,
        ICurrentUserService currentUser,
        IMediator mediator,
        CancellationToken ct)
    {
        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase))
        {
            return Results.Problem("Doctors cannot modify payments.", statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await mediator.Send<Result<PaymentDto>>(
            new RemovePaymentItemCommand(paymentId, itemId), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result.Error);
    }

    private static IResult ToErrorResult(Error error) => error.Type switch
    {
        ErrorType.Unauthorized => Results.Problem(error.Description, statusCode: StatusCodes.Status401Unauthorized),
        ErrorType.NotFound => Results.Problem(error.Description, statusCode: StatusCodes.Status404NotFound),
        ErrorType.Conflict => Results.Problem(error.Description, statusCode: StatusCodes.Status409Conflict),
        _ => Results.Problem(error.Description, statusCode: StatusCodes.Status400BadRequest),
    };
}

internal sealed record CreatePaymentRequest(
    Guid DoctorId,
    Guid HealthPlanId,
    DateOnly ExecutionDate,
    string AppointmentNumber,
    string? AuthorizationCode,
    string BeneficiaryCard,
    string BeneficiaryName,
    string ExecutionLocation,
    string PaymentLocation,
    string? Notes,
    IReadOnlyList<CreatePaymentItemRequest> Items);

internal sealed record UpdatePaymentItemStatusRequest(
    PaymentStatus Status,
    string? Notes);

internal sealed record UpdatePaymentRequest(
    DateOnly ExecutionDate,
    string AppointmentNumber,
    string? AuthorizationCode,
    string BeneficiaryCard,
    string BeneficiaryName,
    string ExecutionLocation,
    string PaymentLocation,
    string? Notes);

internal sealed record AddPaymentItemRequest(
    Guid ProcedureId,
    decimal Value);
