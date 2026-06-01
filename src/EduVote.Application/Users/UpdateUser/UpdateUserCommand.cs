using MediatR;

namespace EduVote.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    string Name,
    string RoleName) : IRequest;
