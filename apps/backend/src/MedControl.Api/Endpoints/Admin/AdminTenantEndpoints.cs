using MedControl.Application.Admin.Commands.SetTenantStatus;
using MedControl.Application.Admin.DTOs;
using MedControl.Application.Admin.Queries.ListAllTenants;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Admin;

public static class AdminTenantEndpoints
{
    public static RouteGroupBuilder MapAdminTenants(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListTenants)
             .WithName("ListAllTenants")
             .RequireAuthorization()
             .Produces<IReadOnlyList<AdminTenantDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/{tenantId:guid}/status", SetTenantStatus)
             .WithName("SetTenantStatus")
             .RequireAuthorization()
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ListTenants(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<AdminTenantDto>>>(
            new ListAllTenantsQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SetTenantStatus(
        Guid tenantId,
        SetTenantStatusRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result>(
            new SetTenantStatusCommand(tenantId, request.IsActive), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.NoContent();
    }

    private static IResult ToErrorResult(Error error)
    {
        if (error.Code.StartsWith("Admin.", StringComparison.Ordinal) &&
            error.Type == ErrorType.Unauthorized)
        {
            return Results.Problem(error.Description, statusCode: 403);
        }

        return error.Type switch
        {
            ErrorType.Unauthorized => Results.Problem(error.Description, statusCode: 401),
            ErrorType.NotFound => Results.Problem(error.Description, statusCode: 404),
            ErrorType.Conflict => Results.Problem(error.Description, statusCode: 409),
            _ => Results.Problem(error.Description, statusCode: 400),
        };
    }
}

internal sealed record SetTenantStatusRequest(bool IsActive);
