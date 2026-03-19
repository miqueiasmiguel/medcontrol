using MedControl.Application.Mediator;
using MedControl.Application.Users.Commands.UpdateProfile;
using MedControl.Application.Users.DTOs;
using MedControl.Application.Users.Queries.GetCurrentUser;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Users;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetCurrentUser)
             .WithName("GetCurrentUser")
             .RequireAuthorization()
             .Produces<UserDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/me/profile", UpdateProfile)
             .WithName("UpdateProfile")
             .RequireAuthorization()
             .Produces<UserDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetCurrentUser(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<UserDto>>(new GetCurrentUserQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<UserDto>>(
            new UpdateProfileCommand(request.DisplayName), ct);

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

internal sealed record UpdateProfileRequest(string? DisplayName);
