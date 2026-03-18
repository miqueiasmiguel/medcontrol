using MedControl.Api.Endpoints.Auth;
using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Application.Tenants.Commands.CreateTenant;
using MedControl.Application.Tenants.Commands.SwitchTenant;
using MedControl.Application.Tenants.DTOs;
using MedControl.Application.Tenants.Queries.GetMyTenants;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Tenants;

public static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenants(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetMyTenants)
             .WithName("GetMyTenants")
             .RequireAuthorization()
             .Produces<IReadOnlyList<TenantDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", CreateTenant)
             .WithName("CreateTenant")
             .RequireAuthorization()
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/switch", SwitchTenant)
             .WithName("SwitchTenant")
             .RequireAuthorization()
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetMyTenants(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<TenantDto>>>(
            new GetMyTenantsQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateTenant(
        CreateTenantRequest request,
        HttpContext ctx,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new CreateTenantCommand(request.Name), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        CookieHelper.SetAuthCookies(ctx, result.Value!);
        return Results.NoContent();
    }

    private static async Task<IResult> SwitchTenant(
        SwitchTenantRequest request,
        HttpContext ctx,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new SwitchTenantCommand(request.TenantId), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        CookieHelper.SetAuthCookies(ctx, result.Value!);
        return Results.NoContent();
    }

    private static IResult ToErrorResult(Error error) => error.Type switch
    {
        ErrorType.Unauthorized => Results.Problem(error.Description, statusCode: 401),
        ErrorType.NotFound => Results.Problem(error.Description, statusCode: 404),
        ErrorType.Conflict => Results.Problem(error.Description, statusCode: 409),
        _ => Results.Problem(error.Description, statusCode: 400),
    };
}

public record CreateTenantRequest(string Name);

public record SwitchTenantRequest(Guid TenantId);
