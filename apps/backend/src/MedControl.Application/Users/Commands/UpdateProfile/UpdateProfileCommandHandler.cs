using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Users.DTOs;
using MedControl.Application.Users.Queries.GetCurrentUser;
using MedControl.Domain.Common;
using MedControl.Domain.Users;

namespace MedControl.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    ICurrentUserService currentUser,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("User.Unauthorized", "User is not authenticated.");

    private static readonly Error NotFound =
        Error.NotFound("User.NotFound", "User not found.");

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<UserDto>(Unauthorized);
        }

        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null)
        {
            return Result.Failure<UserDto>(NotFound);
        }

        user.UpdateProfile(request.DisplayName, user.AvatarUrl);
        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(GetCurrentUserQueryHandler.ToDto(user));
    }
}
