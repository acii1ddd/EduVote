using MediatR;

namespace EduVote.Application.Users.GetUsers;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDetails>>;
