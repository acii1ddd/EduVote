using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Repositories;

namespace EduVote.API.Services.Grpc;

// [Authorize(Roles = Roles.Administrator)]
public class UserService(
    IUserRepository userRepository, 
    IRoleRepository roleRepository) 
    : Users.UsersBase
{
    public override async Task<GetUsersResponse> GetUsers(Empty request, 
        ServerCallContext context)
    {
        var users = await userRepository
            .GetUsersWithRolesAndEducationUnitsAsync(context.CancellationToken);
       
        var response = new GetUsersResponse();
        response.Users.AddRange(users.MapToResponseList());
        
        return response;
    }

    public override async Task<Empty> UpdateUser(
        UpdateUserRequest request,
        ServerCallContext context)
    {
        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(
                Guid.Parse(request.Id),
                context.CancellationToken
            );

        if (user is null)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    "User not found"));
        }

        var role = await roleRepository
            .GetByNameAsync(
                request.Role,
                context.CancellationToken
            );

        if (role is null)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    "Role not found"));
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.RoleId = role.Id;

        await userRepository.UpdateAsync(
            user,
            context.CancellationToken
        );

        return new Empty();
    }
    
    public override async Task<Empty> DeleteUser(
        DeleteUserRequest request,
        ServerCallContext context)
    {
        var userId = Guid.Parse(request.Id);

        await userRepository.DeleteAsync(
            userId,
            context.CancellationToken);

        return new Empty();
    }
}