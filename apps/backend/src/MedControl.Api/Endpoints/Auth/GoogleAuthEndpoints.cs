using MedControl.Application.Auth.Commands.GoogleLogin;
using MedControl.Application.Auth.Commands.GoogleVerifyIdToken;
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
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/verify", GoogleVerify)
             .WithName("GoogleVerify")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GoogleCallback(
        GoogleCallbackRequest request,
        HttpContext ctx,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new GoogleLoginCommand(request.Code, request.RedirectUri), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        CookieHelper.SetAuthCookies(ctx, result.Value!);
        return Results.NoContent();
    }

    private static async Task<IResult> GoogleVerify(
        GoogleVerifyRequest request,
        HttpContext ctx,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new GoogleVerifyIdTokenCommand(request.IdToken), ct);

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

public record GoogleCallbackRequest(string Code, string RedirectUri);
public record GoogleVerifyRequest(string IdToken);
