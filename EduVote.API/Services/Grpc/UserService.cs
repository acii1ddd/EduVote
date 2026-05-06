using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Repositories;

namespace EduVote.API.Services.Grpc;

public class UserService(IUserRepository userRepository) 
    : Users.UsersBase
{
    public override async Task<GetUsersResponse> GetUsers(Empty request, 
        ServerCallContext context)
    {
        var users = await userRepository
            .GetUsersWithRolesAsync(context.CancellationToken);
        
        var response = new GetUsersResponse();
        response.Users.AddRange(users.MapToResponseList());
        
        return response;
    }
}