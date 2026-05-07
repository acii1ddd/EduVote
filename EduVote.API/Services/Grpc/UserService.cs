using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace EduVote.API.Services.Grpc;

[Authorize(Roles = Roles.Administrator)]
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