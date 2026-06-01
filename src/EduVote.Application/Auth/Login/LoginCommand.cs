using MediatR;

namespace EduVote.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
