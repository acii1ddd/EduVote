using EduVote.Application.Users;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Users.GetUsers;

public sealed class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDetails>>
{
    public async Task<IReadOnlyList<UserDetails>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await userRepository
            .GetUsersWithRolesAndEducationUnitsAsync(cancellationToken);

        return users.Select(u => u.ToDetails()).ToList();
    }
}
