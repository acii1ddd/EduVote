using EduVote.API.Dto.Register;
using EduVote.DAL.Postgresql.Repositories;
using EduVote.API.Services.Auth.PasswordHasher;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;

namespace EduVote.API.Services.Auth;

public class RegisterUserService(
    IUserRepository userRepository, 
    IPasswordHasher passwordHasher, 
    IRoleRepository roleRepository)
{
    public async Task<RegisterUserResult> Handle(
        string email, string password, string name, CancellationToken cancellationToken)
    {
        var existing = await userRepository
            .GetByEmailAsync(email, cancellationToken);
        
        if (existing is not null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists, "User with this email already exists"));
        }

        var passwordHash = passwordHasher.Hash(password);

        var studentRole = await roleRepository
            .GetByNameAsync(Roles.Student, cancellationToken);

        if (studentRole is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound, $"Default role '{Roles.Student}' was not configured.."));
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            RoleId =  studentRole.Id,
            Name = name,
            PasswordHash = passwordHash
        };

        var userId = await userRepository
            .AddAsync(newUser, cancellationToken);

        return new RegisterUserResult(userId);
    }
}