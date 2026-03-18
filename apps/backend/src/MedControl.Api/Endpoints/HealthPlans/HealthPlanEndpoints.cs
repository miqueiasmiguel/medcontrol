using MedControl.Application.HealthPlans.Commands.CreateHealthPlan;
using MedControl.Application.HealthPlans.Commands.UpdateHealthPlan;
using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.HealthPlans.Queries.GetHealthPlans;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.HealthPlans;

public static class HealthPlanEndpoints
{
    public static RouteGroupBuilder MapHealthPlans(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetHealthPlans)
             .WithName("GetHealthPlans")
             .RequireAuthorization()
             .Produces<IReadOnlyList<HealthPlanDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", CreateHealthPlan)
             .WithName("CreateHealthPlan")
             .RequireAuthorization()
             .Produces<HealthPlanDto>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}", UpdateHealthPlan)
             .WithName("UpdateHealthPlan")
             .RequireAuthorization()
             .Produces<HealthPlanDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> GetHealthPlans(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<HealthPlanDto>>>(new GetHealthPlansQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateHealthPlan(
        CreateHealthPlanRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<HealthPlanDto>>(
            new CreateHealthPlanCommand(request.Name, request.TissCode), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Created($"/health-plans/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateHealthPlan(
        Guid id,
        UpdateHealthPlanRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<HealthPlanDto>>(
            new UpdateHealthPlanCommand(id, request.Name, request.TissCode), ct);

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

internal sealed record CreateHealthPlanRequest(string Name, string TissCode);
internal sealed record UpdateHealthPlanRequest(string Name, string TissCode);
