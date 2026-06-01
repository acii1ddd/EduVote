using MediatR;

namespace EduVote.Application.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest;
