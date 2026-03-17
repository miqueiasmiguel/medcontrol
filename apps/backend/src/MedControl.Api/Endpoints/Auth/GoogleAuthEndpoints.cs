using MedControl.Application.Auth.Commands.GoogleLogin;
using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Auth;

public static class GoogleAuthEndpoints
{
    public static RouteGroupBuilder MapGoogleAuth(this RouteGroupBuilder group)
    {
        group.MapPost("/callback", GoogleCallback)
             .WithName("GoogleCallback")
             .Produces<AuthTokenDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GoogleCallback(
        GoogleCallbackRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new GoogleLoginCommand(request.Code, request.RedirectUri), ct);

        return ToResult(result);
    }

    private static IResult ToResult<T>(Result<T> result) => result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error.Type switch
        {
            ErrorType.Unauthorized => Results.Problem(result.Error.Description, statusCode: 401),
            ErrorType.NotFound => Results.Problem(result.Error.Description, statusCode: 404),
            ErrorType.Conflict => Results.Problem(result.Error.Description, statusCode: 409),
            _ => Results.Problem(result.Error.Description, statusCode: 400),
        };
}

public record GoogleCallbackRequest(string Code, string RedirectUri);
