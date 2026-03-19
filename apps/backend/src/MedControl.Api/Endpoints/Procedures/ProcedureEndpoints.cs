using MedControl.Application.Mediator;
using MedControl.Application.Procedures.Commands.CreateProcedure;
using MedControl.Application.Procedures.Commands.UpdateProcedure;
using MedControl.Application.Procedures.DTOs;
using MedControl.Application.Procedures.Queries.GetProcedures;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Procedures;

public static class ProcedureEndpoints
{
    public static RouteGroupBuilder MapProcedures(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetProcedures)
             .WithName("GetProcedures")
             .RequireAuthorization()
             .Produces<IReadOnlyList<ProcedureDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", CreateProcedure)
             .WithName("CreateProcedure")
             .RequireAuthorization()
             .Produces<ProcedureDto>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}", UpdateProcedure)
             .WithName("UpdateProcedure")
             .RequireAuthorization()
             .Produces<ProcedureDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> GetProcedures(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<ProcedureDto>>>(new GetProceduresQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateProcedure(
        CreateProcedureRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<ProcedureDto>>(
            new CreateProcedureCommand(request.Code, request.Description, request.Value), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Created($"/procedures/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateProcedure(
        Guid id,
        UpdateProcedureRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<ProcedureDto>>(
            new UpdateProcedureCommand(id, request.Code, request.Description, request.Value), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static IResult ToErrorResult(Error error) => error.Type switch
    {
        ErrorType.Unauthorized => Results.Problem(error.Description, statusCode: 401),
        ErrorType.NotFound => Results.Problem(error.Description, statusCode: 404),
        ErrorType.Conflict => Results.Problem(error.Description, statusCode: 409),
        _ => Results.Problem(error.Description, statusCode: 400),
    };
}

internal sealed record CreateProcedureRequest(string Code, string Description, decimal Value);
internal sealed record UpdateProcedureRequest(string Code, string Description, decimal Value);
