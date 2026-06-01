using EduVote.Application.Auth.Services;
using EduVote.Application.Common;
using EduVote.Application.Users.Services;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokenGenerator)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"User with email '{request.Email}' not found.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "Incorrect credentials");
        }

        var accessToken = accessTokenGenerator
            .GenerateAccessToken(user.Id, user.UserRole.Name);

        return new LoginResult(user.Id, user.UserRole.Name, accessToken);
    }
}
