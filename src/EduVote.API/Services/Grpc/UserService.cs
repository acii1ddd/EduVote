using EduVote.API.Mappers;
using EduVote.API.Services.Auth.PasswordHasher;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories;
using EduVote.DAL.Postgresql.Repositories.Interfaces;

namespace EduVote.API.Services.Grpc;

// [Authorize(Roles = Roles.Administrator)]
public class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher)
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

    public override async Task<UserResponse> CreateUser(
        CreateUserRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument, "Email, password and name are required."));
        }

        var existing = await userRepository
            .GetByEmailAsync(request.Email, context.CancellationToken);

        if (existing is not null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists, "User with this email already exists."));
        }

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? Roles.Student : request.Role;

        var role = await roleRepository
            .GetByNameAsync(roleName, context.CancellationToken);

        if (role is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound, $"Role '{roleName}' was not found."));
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            RoleId = role.Id,
            PasswordHash = passwordHasher.Hash(request.Password),
        };

        await userRepository.AddAsync(newUser, context.CancellationToken);

        // Reload with role navigation so MapToResponse has all data
        var created = await userRepository
            .GetByIdWithEducationUnitsAsync(newUser.Id, context.CancellationToken);

        return created!.MapToResponse();
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
            throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
        }

        var role = await roleRepository
            .GetByNameAsync(request.Role, context.CancellationToken);

        if (role is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Role not found."));
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.RoleId = role.Id;

        await userRepository.UpdateAsync(user, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteUser(
        DeleteUserRequest request,
        ServerCallContext context)
    {
        var userId = Guid.Parse(request.Id);

        await userRepository.DeleteAsync(userId, context.CancellationToken);

        return new Empty();
    }
}
