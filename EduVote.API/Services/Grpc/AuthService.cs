using EduVote.API.Services.Auth;

namespace EduVote.API.Services.Grpc;

public class AuthService(
    RegisterUserService registerUserService, 
    LoginUserService loginUserService) 
    : Authentication.AuthenticationBase
{
    public override async Task<RegisterResponse> Register(
        RegisterRequest request, ServerCallContext context)
    {
        var result = await registerUserService
            .Handle(request.Email, request.Password, context.CancellationToken);
        
        return new RegisterResponse { UserId = result.UserId.ToString() };
    }

    public override async Task<LoginResponse> Login(
        LoginRequest request, ServerCallContext context)
    {
        var result = await loginUserService
            .Handle(request.Email, request.Password, context.CancellationToken);

        return new LoginResponse
        {
            UserId = result.User.Id.ToString(),
            Role = result.User.Role,
            AccessToken = result.User.AccessToken
        };
    }
}
