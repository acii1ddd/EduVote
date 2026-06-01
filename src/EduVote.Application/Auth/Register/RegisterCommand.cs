using MediatR;

namespace EduVote.Application.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string Name) : IRequest<RegisterResult>;
