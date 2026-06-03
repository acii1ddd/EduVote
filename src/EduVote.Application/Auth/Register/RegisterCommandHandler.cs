using EduVote.Application.Common;
using EduVote.Application.Users.Services;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Auth.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRoleRepository roleRepository)
    : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await userRepository
            .GetByEmailAsync(request.Email, cancellationToken);

        if (existing is not null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.AlreadyExists,
                "Пользователь с таким email уже существует");
        }

        var studentRole = await roleRepository
            .GetByNameAsync(Roles.Student, cancellationToken);

        if (studentRole is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Default role '{Roles.Student}' was not configured..");
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            RoleId = studentRole.Id,
            Name = request.Name,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        var userId = await userRepository.AddAsync(newUser, cancellationToken);

        return new RegisterResult(userId);
    }
}
