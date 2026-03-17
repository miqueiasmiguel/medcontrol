using MedControl.Application.Auth.Commands.SendMagicLink;
using MedControl.Application.Auth.Commands.VerifyMagicLink;
using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Auth;

public static class MagicLinkEndpoints
{
    public static RouteGroupBuilder MapMagicLink(this RouteGroupBuilder group)
    {
        group.MapPost("/send", SendMagicLink)
             .WithName("SendMagicLink")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/verify", VerifyMagicLink)
             .WithName("VerifyMagicLink")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> SendMagicLink(
        SendMagicLinkRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send<Unit>(new SendMagicLinkCommand(request.Email), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> VerifyMagicLink(
        VerifyMagicLinkRequest request,
        HttpContext ctx,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new VerifyMagicLinkCommand(request.Token), ct);

        if (!result.IsSuccess)
            return ToErrorResult(result.Error);

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

public record SendMagicLinkRequest(string Email);
public record VerifyMagicLinkRequest(string Token);
