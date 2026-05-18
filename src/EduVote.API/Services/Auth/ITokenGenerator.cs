using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.API.Services.Auth;

public interface ITokenGenerator
{
    public string GenerateAccessToken(Guid userId, Role role);
}