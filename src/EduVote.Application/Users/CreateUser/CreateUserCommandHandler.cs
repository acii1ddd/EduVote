using EduVote.Application.Common;
using EduVote.Application.Users;
using EduVote.Application.Users.Services;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Users.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, UserDetails>
{
    public async Task<UserDetails> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "Email, password and name are required.");
        }

        var existing = await userRepository
            .GetByEmailAsync(request.Email, cancellationToken);

        if (existing is not null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.AlreadyExists,
                "Пользователь с таким email уже существует");
        }

        var roleName = string.IsNullOrWhiteSpace(request.RoleName)
            ? Roles.Student
            : request.RoleName;

        var role = await roleRepository.GetByNameAsync(roleName, cancellationToken);

        if (role is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Role '{roleName}' was not found.");
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            RoleId = role.Id,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        await userRepository.AddAsync(newUser, cancellationToken);

        var created = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        return created!.ToDetails();
    }
}
