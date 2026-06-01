using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.Application.Common;
using EduVote.Application.Users.CreateUser;
using EduVote.Application.Users.DeleteUser;
using EduVote.Application.Users.GetUsers;
using EduVote.Application.Users.UpdateUser;
using MediatR;

namespace EduVote.API.Services.Grpc;

// [Authorize(Roles = Roles.Administrator)]
public class UserService(ISender sender) : Users.UsersBase
{
    public override async Task<GetUsersResponse> GetUsers(
        Empty request,
        ServerCallContext context)
    {
        var users = await sender
            .Send(new GetUsersQuery(), context.CancellationToken);

        var response = new GetUsersResponse();
        response.Users.AddRange(users.MapToResponseList());
        return response;
    }

    public override async Task<UserResponse> CreateUser(
        CreateUserRequest request,
        ServerCallContext context)
    {
        try
        {
            var user = await sender.Send(
                new CreateUserCommand(
                    request.Email,
                    request.Password,
                    request.Name,
                    string.IsNullOrWhiteSpace(request.Role) ? null : request.Role),
                context.CancellationToken);

            return user.MapToResponse();
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<Empty> UpdateUser(
        UpdateUserRequest request,
        ServerCallContext context)
    {
        var userId = IdParser.ParseId(request.Id, "User");

        try
        {
            await sender.Send(
                new UpdateUserCommand(userId, request.Email, request.Name, request.Role),
                context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<Empty> DeleteUser(
        DeleteUserRequest request,
        ServerCallContext context)
    {
        var userId = IdParser.ParseId(request.Id, "User");

        await sender.Send(new DeleteUserCommand(userId), context.CancellationToken);

        return new Empty();
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
