using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                "User not found.");
        }

        var role = await roleRepository.GetByNameAsync(request.RoleName, cancellationToken);

        if (role is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                "Role not found.");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.RoleId = role.Id;

        await userRepository.UpdateAsync(user, cancellationToken);
    }
}
