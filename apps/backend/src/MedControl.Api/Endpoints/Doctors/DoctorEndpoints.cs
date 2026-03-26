using MedControl.Application.Doctors.Commands.CreateDoctor;
using MedControl.Application.Doctors.Commands.LinkDoctorProfileToUser;
using MedControl.Application.Doctors.Commands.UpdateDoctor;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Doctors.Queries.GetDoctors;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Doctors;

public static class DoctorEndpoints
{
    public static RouteGroupBuilder MapDoctors(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetDoctors)
             .WithName("GetDoctors")
             .RequireAuthorization()
             .Produces<IReadOnlyList<DoctorDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", CreateDoctor)
             .WithName("CreateDoctor")
             .RequireAuthorization()
             .Produces<DoctorDto>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}", UpdateDoctor)
             .WithName("UpdateDoctor")
             .RequireAuthorization()
             .Produces<DoctorDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/link-user", LinkDoctorToUser)
             .WithName("LinkDoctorToUser")
             .RequireAuthorization()
             .Produces<DoctorDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> GetDoctors(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<DoctorDto>>>(new GetDoctorsQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateDoctor(
        CreateDoctorRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<DoctorDto>>(
            new CreateDoctorCommand(request.Name, request.Crm, request.CouncilState, request.Specialty), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Created($"/doctors/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateDoctor(
        Guid id,
        UpdateDoctorRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<DoctorDto>>(
            new UpdateDoctorCommand(id, request.Name, request.Crm, request.CouncilState, request.Specialty), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> LinkDoctorToUser(
        Guid id,
        LinkDoctorRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<DoctorDto>>(
            new LinkDoctorProfileToUserCommand(id, request.UserId), ct);

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

internal sealed record CreateDoctorRequest(string Name, string Crm, string CouncilState, string Specialty);
internal sealed record UpdateDoctorRequest(string Name, string Crm, string CouncilState, string Specialty);
internal sealed record LinkDoctorRequest(Guid UserId);
