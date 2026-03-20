using MedControl.Application.Members.Commands.AddMember;
using MedControl.Application.Members.Commands.RemoveMember;
using MedControl.Application.Members.Commands.UpdateMemberRole;
using MedControl.Application.Members.DTOs;
using MedControl.Application.Members.Queries.ListMembers;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Api.Endpoints.Members;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMembers(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListMembers)
             .WithName("ListMembers")
             .RequireAuthorization()
             .Produces<IReadOnlyList<MemberDto>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", AddMember)
             .WithName("AddMember")
             .RequireAuthorization()
             .Produces<MemberDto>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{userId:guid}", UpdateMemberRole)
             .WithName("UpdateMemberRole")
             .RequireAuthorization()
             .Produces<MemberDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}", RemoveMember)
             .WithName("RemoveMember")
             .RequireAuthorization()
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ListMembers(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<MemberDto>>>(new ListMembersQuery(), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> AddMember(
        AddMemberRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<MemberDto>>(
            new AddMemberCommand(request.Email, request.Role), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Created($"/members/{result.Value!.UserId}", result.Value);
    }

    private static async Task<IResult> UpdateMemberRole(
        Guid userId,
        UpdateMemberRoleRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result<MemberDto>>(
            new UpdateMemberRoleCommand(userId, request.Role), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RemoveMember(
        Guid userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send<Result>(new RemoveMemberCommand(userId), ct);

        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error);
        }

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

internal sealed record AddMemberRequest(string Email, string Role);
internal sealed record UpdateMemberRoleRequest(string Role);
