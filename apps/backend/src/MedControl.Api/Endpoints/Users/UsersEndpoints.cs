using MedControl.Application.Doctors.Commands.UpdateMyDoctorProfile;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Doctors.Queries.GetMyDoctorProfile;
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

        group.MapGet("/me/doctor-profile", GetMyDoctorProfile)
             .WithName("GetMyDoctorProfile")
             .RequireAuthorization()
             .Produces<DoctorDto?>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/me/doctor-profile", UpdateMyDoctorProfile)
             .WithName("UpdateMyDoctorProfile")
             .RequireAuthorization()
             .Produces<IReadOnlyList<DoctorDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status409Conflict);

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

    private static async Task<IResult> GetMyDoctorProfile(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<DoctorDto?>>(new GetMyDoctorProfileQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateMyDoctorProfile(
        UpdateMyDoctorProfileRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<DoctorDto>>>(
            new UpdateMyDoctorProfileCommand(request.Name, request.Crm, request.CouncilState, request.Specialty), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static IResult ToErrorResult(Error error) => error.Type switch
    {
        ErrorType.Unauthorized => Results.Problem(error.Description, statusCode: 401),
        ErrorType.Forbidden => Results.Problem(error.Description, statusCode: 403),
        ErrorType.NotFound => Results.Problem(error.Description, statusCode: 404),
        ErrorType.Conflict => Results.Problem(error.Description, statusCode: 409),
        _ => Results.Problem(error.Description, statusCode: 400),
    };
}

internal sealed record UpdateProfileRequest(string? DisplayName);
internal sealed record UpdateMyDoctorProfileRequest(string Name, string Crm, string CouncilState, string Specialty);
