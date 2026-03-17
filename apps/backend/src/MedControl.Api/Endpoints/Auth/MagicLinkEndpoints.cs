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
             .Produces<AuthTokenDto>(StatusCodes.Status200OK)
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
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<AuthTokenDto>>(
            new VerifyMagicLinkCommand(request.Token), ct);

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

public record SendMagicLinkRequest(string Email);
public record VerifyMagicLinkRequest(string Token);
