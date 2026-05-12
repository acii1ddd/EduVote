using EduVote.API.Dto.Login;
using EduVote.API.Services.Auth.PasswordHasher;
using EduVote.DAL.Postgresql.Repositories;
using EduVote.DAL.Postgresql.Repositories.Interfaces;

namespace EduVote.API.Services.Auth;

public class LoginUserService(
    IUserRepository userRepository, 
    IPasswordHasher passwordHasher, 
    ITokenGenerator tokenGenerator)
{
    public async Task<LoginUserResult> Handle(
        string email, string password, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound, $"User with email '{email}' not found."));
        }

        if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument, "Incorrect credentials"));
        }

        var accessToken = tokenGenerator
            .GenerateAccessToken(user.Id, user.UserRole);

        var loginUserDto = new LoginUserDto(user.Id, user.UserRole.Name, accessToken);
        return new LoginUserResult(loginUserDto);
    }
}