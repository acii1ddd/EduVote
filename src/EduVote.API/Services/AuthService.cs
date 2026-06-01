using EduVote.Application.Auth.Login;
using EduVote.Application.Auth.Register;
using EduVote.Application.Common;
using MediatR;

namespace EduVote.API.Services;

public class AuthService(ISender sender) : Authentication.AuthenticationBase
{
    public override async Task<RegisterResponse> Register(
        RegisterRequest request,
        ServerCallContext context)
    {
        try
        {
            var result = await sender.Send(
                new RegisterCommand(request.Email, request.Password, request.Name),
                context.CancellationToken);

            return new RegisterResponse { UserId = result.UserId.ToString() };
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<LoginResponse> Login(
        LoginRequest request,
        ServerCallContext context)
    {
        try
        {
            var result = await sender.Send(
                new LoginCommand(request.Email, request.Password),
                context.CancellationToken);

            return new LoginResponse
            {
                UserId = result.UserId.ToString(),
                Role = result.Role,
                AccessToken = result.AccessToken
            };
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    private static StatusCode MapStatusCode(ApplicationErrorType errorType) =>
        errorType switch
        {
            ApplicationErrorType.InvalidArgument => StatusCode.InvalidArgument,
            ApplicationErrorType.NotFound => StatusCode.NotFound,
            ApplicationErrorType.PermissionDenied => StatusCode.PermissionDenied,
            ApplicationErrorType.FailedPrecondition => StatusCode.FailedPrecondition,
            ApplicationErrorType.AlreadyExists => StatusCode.AlreadyExists,
            ApplicationErrorType.Unauthenticated => StatusCode.Unauthenticated,
            ApplicationErrorType.Unavailable => StatusCode.Unavailable,
            _ => StatusCode.Unknown
        };
}
