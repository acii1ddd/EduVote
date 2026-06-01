using MediatR;

namespace EduVote.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string Name,
    string? RoleName) : IRequest<UserDetails>;
